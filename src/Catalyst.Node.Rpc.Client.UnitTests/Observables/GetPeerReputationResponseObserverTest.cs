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
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalyst.Common.Config;
using Catalyst.Common.Interfaces.Cli;
using Catalyst.Common.IO.Messaging;
using Catalyst.Common.IO.Messaging.Dto;
using Catalyst.Node.Rpc.Client.Observables;
using Catalyst.Protocol.Rpc.Node;
using Catalyst.TestUtils;
using DotNetty.Transport.Channels;
using NSubstitute;
using Serilog;
using Xunit;

namespace Catalyst.Node.Rpc.Client.UnitTests.Observables
{
    /// <summary>
    /// Tests the CLI for peer reputation response
    /// </summary>
    public sealed class GetPeerReputationResponseObserverTest : IDisposable
    {
        private readonly IUserOutput _output;
        public static readonly List<object[]> QueryContents;
        private readonly IChannelHandlerContext _fakeContext;

        private readonly ILogger _logger;
        private PeerReputationResponseObserver _observer;

        /// <summary>
        /// Initializes the <see cref="GetPeerReputationResponseObserverTest"/> class.
        /// </summary>
        static GetPeerReputationResponseObserverTest()
        {             
            QueryContents = new List<object[]>
            {
                new object[] {78},
                new object[] {1572},
                new object[] {22}
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPeerReputationResponseObserverTest"/> class.
        /// </summary>
        public GetPeerReputationResponseObserverTest()
        {
            _logger = Substitute.For<ILogger>();
            _fakeContext = Substitute.For<IChannelHandlerContext>();
            _output = Substitute.For<IUserOutput>();
        }

        /// <summary>
        /// RPCs the client can handle get reputation response.
        /// </summary>
        /// <param name="rep">The rep.</param>
        [Theory]
        [MemberData(nameof(QueryContents))]
        public async Task RpcClient_Can_Handle_GetReputationResponse(int rep)
        {
            await TestGetReputationResponse(rep);

            _output.Received(1).WriteLine($"Peer Reputation: {rep}");
        }

        /// <summary>
        /// RPCs the client can handle get reputation response non existant peers.
        /// </summary>
        /// <param name="rep">The rep.</param>
        [Theory]
        [InlineData(int.MinValue)]
        public async Task RpcClient_Can_Handle_GetReputationResponseNonExistantPeers(int rep)
        {
            await TestGetReputationResponse(rep);

            _output.Received(1).WriteLine("Peer Reputation: Peer not found");
        }

        private async Task TestGetReputationResponse(int rep)
        {
            var response = new MessageFactory().GetMessage(new MessageDto(
                    new GetPeerReputationResponse
                    {
                        Reputation = rep
                    },
                    MessageTypes.Request,
                    PeerIdentifierHelper.GetPeerIdentifier("recpient"),
                    PeerIdentifierHelper.GetPeerIdentifier("sender")),
                Guid.NewGuid());

            var messageStream = MessageStreamHelper.CreateStreamWithMessage(_fakeContext, response);

            _observer = new PeerReputationResponseObserver(_output, _logger);
            _observer.StartObserving(messageStream);

            await messageStream.WaitForEndOfDelayedStreamOnTaskPoolScheduler();
        }

        public void Dispose()
        {
            _observer?.Dispose();
        }
    }
}