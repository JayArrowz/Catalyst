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

using System.IO;
using System.Linq;
using Autofac;
using Catalyst.Common.Config;
using Catalyst.Common.Extensions;
using Catalyst.Common.Interfaces.IO.Messaging;
using Catalyst.Common.Interfaces.Modules.KeySigner;
using Catalyst.Common.IO.Messaging;
using Catalyst.Common.IO.Messaging.Dto;
using Catalyst.Common.Util;
using Catalyst.Node.Core.RPC.Observables;
using Catalyst.Protocol.Common;
using Catalyst.Protocol.Rpc.Node;
using Catalyst.TestUtils;
using DotNetty.Transport.Channels;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Nethereum.RLP;
using NSubstitute;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Catalyst.Node.Core.UnitTests.RPC.Observables
{
    public sealed class VerifyMessageRequestObserverTests : ConfigFileBasedTest
    {
        private readonly ILifetimeScope _scope;
        private readonly ILogger _logger;
        private readonly IKeySigner _keySigner;
        private readonly IChannelHandlerContext _fakeContext;
        private readonly IMessageFactory _messageFactory;

        public VerifyMessageRequestObserverTests(ITestOutputHelper output) : base(output)
        {
            var config = SocketPortHelper.AlterConfigurationToGetUniquePort(new ConfigurationBuilder()
               .AddJsonFile(Path.Combine(Constants.ConfigSubFolder, Constants.ComponentsJsonConfigFile))
               .AddJsonFile(Path.Combine(Constants.ConfigSubFolder, Constants.SerilogJsonConfigFile))
               .AddJsonFile(Path.Combine(Constants.ConfigSubFolder, Constants.NetworkConfigFile(Network.Dev)))
               .AddJsonFile(Path.Combine(Constants.ConfigSubFolder, Constants.ShellNodesConfigFile))
               .Build(), CurrentTestName);

            ConfigureContainerBuilder(config);

            var container = ContainerBuilder.Build();
            _scope = container.BeginLifetimeScope(CurrentTestName);
            
            _keySigner = container.Resolve<IKeySigner>();
            _messageFactory = Substitute.For<IMessageFactory>();
            _logger = Substitute.For<ILogger>();
            _fakeContext = Substitute.For<IChannelHandlerContext>();
            
            var fakeChannel = Substitute.For<IChannel>();
            _fakeContext.Channel.Returns(fakeChannel);
        }
        
        [Theory(Skip = "Fails, fixing in #393")]
        [InlineData("hello", "mL9Z+e5gIfEdfhDWUxkUox886YuiZnhEj3om5AXmWVXJK7dl7/ESkjhbkJsrbzIbuWm8EPSjJ2YicTIcXvfzIAw", "zGfHq2tTVk9z4eXgyUwcss5uApFrvVdAjf395XdQt2wbY8drESxbLQSHrbSx2", true)]
        [InlineData("Different Message", "mL9Z+e5gIfEdfhDWUxkUox886YuiZnhEj3om5AXmWVXJK7dl7/ESkjhbkJsrbzIbuWm8EPSjJ2YicTIcXvfzIAw", "zGfHq2tTVk9z4eXgyUwcss5uApFrvVdAjf395XdQt2wbY8drESxbLQSHrbSx2", false)]
        [InlineData("hello", "any signature", "zGfHq2tTVk9z4eXgyUwcss5uApFrvVdAjf395XdQt2wbY8drESxbLQSHrbSx2", false)]
        [InlineData("hello", "mL9Z+e5gIfEdfhDWUxkUox886YuiZnhEj3om5AXmWVXJK7dl7/ESkjhbkJsrbzIbuWm8EPSjJ2YicTIcXvfzIAw", "any public key", false)]
        [InlineData("hello", "any signature", "any public key", false)]
        [InlineData("", "", "", false)]
        public void VerifyMessageRequest_UsingValidRequest_ShouldSendVerifyMessageResponse(string message, string signature, string publicKey, bool expectedResult)
        {
            var request = new MessageFactory().GetMessage(new MessageDto(
                new VerifyMessageRequest
                {
                    Message = RLP.EncodeElement(message.Trim('\"').ToBytesForRLPEncoding()).ToByteString(),
                    PublicKey = publicKey.ToBytesForRLPEncoding().ToByteString(),
                    Signature = signature.ToBytesForRLPEncoding().ToByteString()
                },
                MessageTypes.Request,
                PeerIdentifierHelper.GetPeerIdentifier("recipient_key"),
                PeerIdentifierHelper.GetPeerIdentifier("sender_key")
            ));
            
            var messageStream = MessageStreamHelper.CreateStreamWithMessage(_fakeContext, request);
            var handler = new VerifyMessageRequestObserver(PeerIdentifierHelper.GetPeerIdentifier("sender"), _logger, _keySigner, _messageFactory);
            handler.StartObserving(messageStream);
            
            var receivedCalls = _fakeContext.Channel.ReceivedCalls().ToList();
            receivedCalls.Count.Should().Be(1);
            
            var sentResponse = (ProtocolMessage) receivedCalls.Single().GetArguments().Single();
            sentResponse.TypeUrl.Should().Be(VerifyMessageResponse.Descriptor.ShortenedFullName());

            var responseContent = sentResponse.FromProtocolMessage<VerifyMessageResponse>();

            responseContent.IsSignedByKey.Should().Be(expectedResult);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
            {
                return;
            }
            
            _scope?.Dispose();
        }
    }
}