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
using Catalyst.Abstractions.Kvm.Models;
using Catalyst.Abstractions.Ledger;
using Catalyst.Protocol.Deltas;
using LibP2P;

namespace Catalyst.Core.Modules.Web3 
{
    public static class Web3EthApiExtensions
    {
        public static Delta GetLatestDelta(this IWeb3EthApi api)
        {
            return GetDelta(api, api.DeltaResolver.LatestDelta);
        }

        public static Delta GetDelta(this IWeb3EthApi api, Cid cid)
        {
            if (!api.DeltaCache.TryGetOrAddConfirmedDelta(cid, out Delta delta))
            {
                throw new Exception($"Delta not found '{cid}'");
            }

            return delta;
        }

        public static Delta GetDelta(this IWeb3EthApi api, BlockParameter block)
        {
            Cid cid;
            switch (block.Type)
            {
                case BlockParameterType.Earliest:
                    cid = api.DeltaCache.GenesisHash;
                    break;
                case BlockParameterType.Latest:
                    cid = api.DeltaResolver.LatestDelta;
                    break;
                case BlockParameterType.Pending:
                    cid = api.DeltaResolver.LatestDelta;
                    break;
                case BlockParameterType.BlockNumber:
                    var blockNumber = block.BlockNumber.Value;
                    cid = api.DeltaResolver.Resolve(blockNumber);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return api.GetDelta(cid);
        }
    }
}