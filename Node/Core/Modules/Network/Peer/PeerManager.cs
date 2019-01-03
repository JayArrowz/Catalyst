using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ADL.Hex.HexConvertors.Extensions;
using ADL.Network;
using ADL.Node.Core.Modules.Network.Listeners;
using ADL.Node.Core.Modules.Network.Messages;
using ADL.Protocol.Peer;
using Google.Protobuf;
using Org.BouncyCastle.Security;

namespace ADL.Node.Core.Modules.Network.Peer
{
    /// <summary>
    /// 
    /// </summary>
    public class PeerManager : IDisposable
    {
        private int ActiveConnections;
        private bool Disposed  { get; set; }
        private PeerList PeerList  { get; set; }
        private TcpListener Listener { get; set; }
        private CancellationToken Token { get; set; }
        private bool AcceptInvalidCerts { get; set; }
        private X509Certificate2 SslCertificate { get; set; }
        private MessageQueueManager MessageQueueManager { get; set; }
        private CancellationTokenSource CancellationToken { get; set; }

        public PeerManager(X509Certificate2 sslCertificate, PeerList peerList, MessageQueueManager messageQueueManager)
        {
            PeerList = peerList;
            ActiveConnections = 0;
            SslCertificate = sslCertificate;
            MessageQueueManager = messageQueueManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        private async Task<bool> DataReceiver(Connection connection, CancellationToken cancelToken)
        {
            var streamReadCounter = 0;
            var port = ((IPEndPoint) connection.TcpClient.Client.LocalEndPoint).Port;
            var ip = ((IPEndPoint) connection.TcpClient.Client.LocalEndPoint).Address.ToString();

            if (connection == null) throw new ArgumentNullException(nameof(connection));
            
            if (PeerList.UnIdentifiedPeers.TryRemove(ip+":"+port, out Connection removedConnection))
            {
                Log.Log.Message(removedConnection + "Connection already exists");
                return false;
            }

            if (PeerList.UnIdentifiedPeers.TryAdd(ip+":"+port, connection))
            {
                int activeCount = Interlocked.Increment(ref ActiveConnections);
                Log.Log.Message("*** FinalizeConnection starting data receiver for " + ip + port + " (now " + activeCount + " connections)");
            }
            else
            {
                connection.Dispose();
                return false;
            }
            
            try
            {
                while (true)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    if (!connection.IsConnected())
                    {
                        Log.Log.Message("*** Data receiver can not attach to connection");
                        break;
                    }

                    byte[] payload = Stream.Reader.MessageRead(connection.SslStream);

                    if (payload == null)
                    {
                        await Task.Delay(30, Token);
                        streamReadCounter += streamReadCounter;
                        // count how many times we try reading header && content so we don't get stuck in here.
                        if (streamReadCounter == 5)
                        {
                            break;
                        }
                    }
                    else
                    {
                        lock (MessageQueueManager._receivedMessageQueue)
                        {
                            MessageQueueManager._receivedMessageQueue.Enqueue(payload);
                            Log.Log.Message("messages in queue: " + MessageQueueManager._receivedMessageQueue.Count);
                        }
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Log.LogException.Message("*** Data receiver cancelled " + ip + ":" + port + " disconnected", e);
                throw;
            }
            catch (Exception e)
            {
                Log.LogException.Message("*** Data receiver exception " + ip + ":" + port + " disconnected", e);
                throw;
            }
            finally
            {                
                await Task.Run(() => DisconnectConnection(connection), Token);
            }
            return true;
        }

        /// <summary>
        /// @TODO we need to announce our node to trackers.
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="Exception"></exception>
        internal async Task InboundConnectionListener(IPEndPoint ipEndPoint)
        {
            Listener = ListenerFactory.CreateTcpListener(ipEndPoint);

            Listener.Start();
            Log.Log.Message("Peer server starting on " + ipEndPoint.Address + ":" + ipEndPoint.Port );
   
            while (!Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient tcpClient = await Listener.AcceptTcpClientAsync();
                    tcpClient.LingerState.Enabled = false;

                    try
                    {
                        if (PeerList.CheckIfIpBanned(tcpClient))
                        {
                            // incoming endpoint is in banned list so peace out bro! ☮ ☮ ☮ ☮ 
                            tcpClient.Dispose();
                            continue;
                        }
                    }
                    catch (ArgumentNullException e)
                    {
                        tcpClient.Dispose();
                        Log.LogException.Message("InboundConnectionListener: CheckIfIpBanned", e);
                        continue;
                    }

                    Connection connection;
                    try
                    {
                        connection = StartPeerConnection(tcpClient);
                        if (connection == null) continue;
                    }
                    catch (Exception e)
                    {
                        Log.LogException.Message("InboundConnectionListener: StartPeerConnection", e);
                        continue;
                    }

                    using (connection)
                    {
                        try
                        {
                            connection.SslStream = GetPeerConnectionTlsStream(connection);
                        }
                        catch (Exception e)
                        {
                            DisconnectConnection(connection);
                            Log.LogException.Message("InboundConnectionListener: GetPeerConnectionTlsStream", e);
                            continue;
                        }

                        if (await DataReceiver(connection, Token))
                        {
                            Log.Log.Message("*** AcceptConnections accepted connection from " + connection.EndPoint.Address + connection.EndPoint.Port + " count " + ActiveConnections);
                            Log.Log.Message("Starting Challenge Request");
                            PeerProtocol.Types.ChallengeRequest requestMessage = MessageFactory.Get(2);

                            SecureRandom random = new SecureRandom();
                            byte[] keyBytes = new byte[16];
                            random.NextBytes(keyBytes);
                            requestMessage.Nonce = random.NextInt();
                            if (connection.SslStream != null)
                            {
//                            connection.Nonce = requestMessage.Nonce;
                                byte[] requestBytes = requestMessage.ToByteArray();
                                Console.WriteLine(requestMessage);
                                Console.WriteLine(requestBytes.ToHex());
                                Stream.Writer.MessageWrite(connection, requestBytes, 98);
                            }
                            continue;
                        }
                        Log.Log.Message("*** FinalizeConnection unable to add peer " + connection.EndPoint.Address + connection.EndPoint.Port);
                        throw new Exception("unable to add connection as peer");                        
                    }

                }
                catch (AuthenticationException e)
                {
                    Log.LogException.Message("InboundConnectionListener AuthenticationException", e);
                }
                catch (ObjectDisposedException ex)
                {
//                    Log.Log.Message("*** AcceptConnections ObjectDisposedException from " + ListenerIpAddress + Environment.NewLine +ex);
                }
                catch (SocketException ex)
                {
                    switch (ex.Message)
                    {
                        case "An existing connection was forcibly closed by the remote host":
//                            Log.Log.Message("*** AcceptConnections SocketException " + ListenerIpAddress + " closed the connection.");
                            break;
                        default:
//                            Log.Log.Message("*** AcceptConnections SocketException from " + peerIp.Ip + Environment.NewLine + ex);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogException.Message("*** AcceptConnections Exception from ", ex);
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="AuthenticationException"></exception>
        public async void BuildOutBoundConnection (string ip, int port)
        {
            if (string.IsNullOrEmpty(ip)) throw new ArgumentNullException(nameof(ip));
            if (Ip.ValidPortRange(port)) throw new ArgumentOutOfRangeException(nameof(port));

            try
            {
                using (TcpClient tcpClient = new TcpClient())
                { 
                    try
                    {
                        IPEndPoint targetEndpoint = EndpointBuilder.BuildNewEndPoint(ip, port);
                        IAsyncResult asyncClient = tcpClient.BeginConnect(targetEndpoint.Address, targetEndpoint.Port, null, null);
                        WaitHandle asyncClientWaitHandle = asyncClient.AsyncWaitHandle;
                        
                        try
                        {
                            if (!asyncClient.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
                            {
                                tcpClient.Close();
                                throw new TimeoutException("Timeout connecting to " +  targetEndpoint.Address + ":" + targetEndpoint.Port);
                            }

                            tcpClient.EndConnect(asyncClient);

                            Connection connection = StartPeerConnection(tcpClient);

                            connection.SslStream = GetPeerConnectionTlsStream(connection);

                            if (await DataReceiver(connection, Token)) return;
                            throw new Exception("*** FinalizeConnection unable to add peer " + connection.EndPoint.Address + connection.EndPoint.Port);
                        }
                        catch (AuthenticationException e)
                        {
                            Log.LogException.Message("Peer builder socket exception", e);
                        }
                        finally
                        {
                            asyncClientWaitHandle.Close();
                        }
                    }
                    catch (ArgumentNullException e)
                    {
                        Log.LogException.Message("ADL.Node.Core.Modules.Network.Peer.PeerManager.PeerBuilder", e);
                    }
                }
            }
            catch (ArgumentException e)
            {
                Log.LogException.Message("ADL.Node.Core.Modules.Network.Peer.PeerManager.PeerBuilder", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Connection StartPeerConnection(TcpClient tcpClient)
        {
            Connection connection;
            try
            {
                connection = new Connection(tcpClient);
            }
            catch (Exception e)
            {
                Log.LogException.Message("InboundConnectionListener", e);
                throw new Exception(e.Message);
            }

            int activeCount = Interlocked.Increment(ref ActiveConnections);
            Log.Log.Message("*** Connection created for " + connection.EndPoint.Address + connection.EndPoint.Port + " (now " + activeCount + " connections)");
            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private SslStream GetPeerConnectionTlsStream(Connection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            
            SslStream sslStream = Stream.StreamFactory.CreateTlsStream(
                connection.NetworkStream,
                1,
                SslCertificate,
                AcceptInvalidCerts
            );
                    
            if (connection.SslStream == null || connection.SslStream.GetType() != typeof (SslStream))
            {
                throw new Exception("Peer ssl stream not set");
            }

            if (!PeerList.AddUnidentifiedConnectionToList(connection))
            {
                connection.Dispose();
                throw new Exception("Connection already established");
            }
            
            return sslStream;
        }
        
        
        /// <summary>
        /// Disconnects a connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private bool DisconnectConnection(Connection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof (connection));

            try
            {
                // first check our unidentified connections
                if (!PeerList.RemoveUnidentifiedConnectionFromList(connection))
                {
                    // its not in our unidentified list so now check the peer bucket
                    Peer peer = PeerList.FindPeerFromConnection(connection);
                    if (peer == null) throw new ArgumentNullException(nameof(peer));
                    if (PeerList.RemovePeerFromBucket(peer))
                    {
                        peer.Dispose();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    connection.Dispose();    
                }

                var activeCount = Interlocked.Decrement(ref ActiveConnections);
                Log.Log.Message("***** Successfully disconnected " + connection.EndPoint.Address + connection.EndPoint.Port + " connected (now " + activeCount + " connections active)");

                return true;
            }
            catch (Exception e)
            {
                Log.LogException.Message("DisconnectConnection: RemoveUnidentifiedConnectionFromList", e);
                return false;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            Log.Log.Message("disposing network class");
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// dispose server and background workers.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                CancellationToken.Cancel();
                CancellationToken.Dispose();

                if (Listener?.Server != null)
                {
                    Listener.Server.Close();
                    Listener.Server.Dispose();
                }
                
                if (PeerList.UnIdentifiedPeers?.Count > 0)
                {
                    foreach (KeyValuePair<string, Connection> peer in PeerList.UnIdentifiedPeers)
                    {
                        peer.Value.Dispose();
                    }
                }
                
                if (PeerList.PeerBucket?.Count > 0)
                {
                    foreach (KeyValuePair<PeerIdentifier, Peer> peer in PeerList.PeerBucket)
                    {
                        peer.Value.Dispose();
                    }
                }
            }
            
            Disposed = true;    
            Log.Log.Message("Network class disposed");
        }
    }
}
