#region LICENSE

/**
* Copyright (c) 2019 Catalyst Network
*
* This file is part of Catalyst.Node <https://github.com/catalyst-network/Catalyst.Node>
*
* Catalyst.Node is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 2 of the License, or
* (at your option) any later version.
*
* Catalyst.Node is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Catalyst.Node. If not, see <https://www.gnu.org/licenses/>.
*/

#endregion

using Catalyst.Abstractions.Dfs.CoreApi;
using Catalyst.Abstractions.IO.Handlers;
using Catalyst.Abstractions.IO.Messaging.Dto;
using Catalyst.Abstractions.KeySigner;
using Catalyst.Abstractions.P2P;
using Catalyst.Abstractions.P2P.IO.Messaging.Correlation;
using Catalyst.Core.Lib.Extensions;
using Catalyst.Core.Lib.IO.LibP2PHandlers;
using Catalyst.Protocol.Cryptography;
using Catalyst.Protocol.Wire;
using Google.Protobuf;
using Lib.P2P.Protocols;
using MultiFormats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalyst.Core.Lib.P2P
{
    public class LibP2PPeerClient : ILibP2PPeerClient
    {
        private readonly IPeerSettings _peerSettings;
        private readonly IPubSubApi _pubSubApi;
        private readonly ICatalystProtocol _catalystProtocol;
        private readonly SigningContext _signingContext;
        private readonly IList<IMessageHandler> _handlers;

        public IObservable<ProtocolMessage> MessageStream { private set; get; }

        /// <param name="clientChannelFactory">A factory used to build the appropriate kind of channel for a udp client.</param>
        /// <param name="eventLoopGroupFactory"></param>
        /// <param name="peerSettings"></param>
        public LibP2PPeerClient(
            IPeerMessageCorrelationManager messageCorrelationManager,
            IKeySigner keySigner,
            IPeerSettings peerSettings,
            IPubSubApi pubSubApi,
            ICatalystProtocol catalystProtocol)
        {
            _peerSettings = peerSettings;
            _pubSubApi = pubSubApi;
            _catalystProtocol = catalystProtocol;
            _signingContext = new SigningContext { NetworkType = peerSettings.NetworkType, SignatureType = SignatureType.ProtocolPeer };

            _handlers = new List<IMessageHandler>
            {
                new ProtocolMessageSignHandler(keySigner, _signingContext),
                new CorrelatableHandler<IPeerMessageCorrelationManager>(messageCorrelationManager)
            };
        }

        public async Task StartAsync()
        {

        }

        public async Task SendMessageToPeersAsync(IMessage message, IEnumerable<MultiAddress> peers)
        {
            var protocolMessage = message.ToProtocolMessage(_peerSettings.Address);
            foreach (var peer in peers)
            {
                await SendMessageAsync(peer, protocolMessage).ConfigureAwait(false);
            }
        }

        public async Task SendMessageAsync<T>(IMessageDto<T> message) where T : IMessage<T>
        {
            var protocolMessage = ProtocolMessage.Parser.ParseFrom(message.Content.ToByteArray());
            await SendMessageAsync(message.RecipientPeerIdentifier, protocolMessage).ConfigureAwait(false);
        }

        private async Task SendMessageAsync(MultiAddress receiver, ProtocolMessage message)
        {
            try
            {
                foreach (var handler in _handlers)
                {
                    var result = await handler.ProcessAsync(message).ConfigureAwait(false);
                    if (!result)
                    {
                        return;
                    }
                }

                await _catalystProtocol.SendAsync(receiver, message).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                //Peer does not support catalyst protocol
            }
        }

        public async Task BroadcastAsync(ProtocolMessage message)
        {
            foreach (var handler in _handlers)
            {
                var result = await handler.ProcessAsync(message).ConfigureAwait(false);
                if (!result)
                {
                    return;
                }
            }

            await _pubSubApi.PublishAsync("catalyst", message.ToByteArray()).ConfigureAwait(false);
        }
    }
}

