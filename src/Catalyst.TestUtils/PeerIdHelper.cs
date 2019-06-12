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

using System;
using System.Linq;
using System.Net;
using System.Text;
using Catalyst.Common.Extensions;
using Catalyst.Common.Network;
using Catalyst.Common.Util;
using Catalyst.Protocol.Common;

namespace Catalyst.TestUtils
{
    public static class PeerIdHelper
    {
        public static PeerId GetPeerId(byte[] publicKey = null,
            string clientId = "Tc",
            int clientVersion = 1,
            IPAddress ipAddress = null,
            int port = 12345)
        {
            var peerIdentifier = new PeerId()
            {
                PublicKey = (publicKey ?? new byte[32]).ToByteString(),
                ClientId = clientId.ToUtf8ByteString(),
                ClientVersion = clientVersion.ToString("D2").ToUtf8ByteString(),
                Ip = (ipAddress ?? IPAddress.Parse("127.0.0.1")).To16Bytes().ToByteString(),
                Port = BitConverter.GetBytes((ushort) port).ToByteString()
            };
            return peerIdentifier;
        }

        public static PeerId GetPeerId(string publicKeySeed,
            string clientId = "Tc",
            int clientVersion = 1,
            IPAddress ipAddress = null,
            int port = 12345)
        {
            var publicKeyBytes = Encoding.UTF8.GetBytes(publicKeySeed)
               .Concat(Enumerable.Repeat(default(byte), 32))
               .Take(32).ToArray();
            return GetPeerId(publicKeyBytes, clientId, clientVersion, ipAddress, port);
        }
    }
}