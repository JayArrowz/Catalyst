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

using Catalyst.Common.Interfaces.IO.Messaging.Correlation;
using Catalyst.Common.Interfaces.IO.Observers;
using Catalyst.Common.Interfaces.P2P;
using Catalyst.Common.Interfaces.Rpc.IO.Messaging.Dto;
using Catalyst.Common.IO.Observers;
using Catalyst.Node.Rpc.Client.IO.Messaging.Dto;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Catalyst.Node.Rpc.Client.IO
{
    public abstract class RpcResponseObserver<TProto> : ResponseObserverBase<TProto>, IRpcResponseObserver<TProto> where TProto : IMessage<TProto>
    {
        private readonly ConcurrentBag<IDisposable> _messageResponseSubscriptions;
        private readonly ReplaySubject<IRpcClientMessageDto<IMessage>> _messageResponse;
        public IObservable<IRpcClientMessageDto<IMessage>> MessageResponseStream { private set; get; }

        protected RpcResponseObserver(ILogger logger, bool assertMessageNameCheck = true) : base(logger, assertMessageNameCheck)
        {
            _messageResponseSubscriptions = new ConcurrentBag<IDisposable>();
            _messageResponse = new ReplaySubject<IRpcClientMessageDto<IMessage>>(1);
            MessageResponseStream = _messageResponse.AsObservable();
        }

        protected override void RedirectResponse(TProto messageDto, IChannelHandlerContext channelHandlerContext, IPeerIdentifier senderPeerIdentifier, ICorrelationId correlationId)
        {
            base.RedirectResponse(messageDto, channelHandlerContext, senderPeerIdentifier, correlationId);
            AddMessageToResponseStream(messageDto, senderPeerIdentifier);
        }

        private void AddMessageToResponseStream(TProto message, IPeerIdentifier senderPeerIdentifier)
        {
            _messageResponse.OnNext(new RpcClientMessageDto<IMessage>(message, senderPeerIdentifier));
        }

        public void SubscribeToResponse(Action<TProto> onNext)
        {
            _messageResponseSubscriptions.Add(MessageResponseStream.Where(x => x.Message is TProto).SubscribeOn(NewThreadScheduler.Default).Subscribe(rpcClientMessageDto =>
                onNext((TProto) rpcClientMessageDto.Message)
            ));
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var subscription in _messageResponseSubscriptions)
            {
                subscription?.Dispose();
            }

            _messageResponseSubscriptions.Clear();
            _messageResponse?.Dispose();
            base.Dispose(disposing);
        }
    }
}