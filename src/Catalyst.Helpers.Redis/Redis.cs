using System;
using System.Collections.Generic;
using System.Net;
using Catalyst.Helpers.KeyValueStore;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace Catalyst.Helpers.Redis
{
    public class Redis : IKeyValueStore
    {
        private When _when;
        private static IPAddress Host { get; set; }
        private readonly IDatabase _redisDb = RedisConnector.GetInstance(Host).GetDb;

        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="when"></param>
        public Redis(IPAddress host, When when = When.NotExists)
        {
            _when = when;
            Host = host;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="when"></param>
        /// <returns></returns>
        public bool Set(byte[] key, byte[] value, TimeSpan? expiry)
        {
            
            return _redisDb.StringSet(key, value, expiry, _when);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] Get(byte[] value)
        {
            return _redisDb.StringGet(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Dictionary<string,string></returns>
        /// <see>https://redis.io/commands/INFO</see>
        public Dictionary<string, string> GetInfo()
        {
            var serializer = new NewtonsoftSerializer();
            var sut = new StackExchangeRedisCacheClient(RedisConnector.GetInstance(Host).Connection, serializer);
            
            return sut.GetInfo();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="when"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ParseSettings(string  when)
        {            
            if (!Enum.TryParse(when, out _when))
            {
                throw new ArgumentException($"Invalid When setting format:{when}");
            }
        }
    }
}