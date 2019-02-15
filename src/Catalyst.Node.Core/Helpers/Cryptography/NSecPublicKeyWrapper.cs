using System;
using NSec.Cryptography;

namespace Catalyst.Node.Core.Helpers.Cryptography
{
    /// <summary>
    /// NSec specific public key wrapper.
    /// </summary>
    public sealed class NSecPublicKeyWrapper : IPublicKey{

        private readonly PublicKey _key;
        public NSecPublicKeyWrapper(PublicKey key)
        {
            _key = key;
        }
  
        
        public PublicKey GetNSecFormatPublicKey()
        {
            return _key;
        }
    } 
}