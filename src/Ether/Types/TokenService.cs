using Blazor.Extensions.Storage;
using System.Threading.Tasks;

namespace Ether.Types
{
    public class TokenService
    {
        private const string TokenKey = "___EtherToken___";
        private readonly LocalStorage _storage;

        public TokenService(LocalStorage storage)
        {
            _storage = storage;
        }

        public ValueTask<AccessToken> GetToken()
        {
            return _storage.GetItem<AccessToken>(TokenKey);
        }

        public ValueTask SetToken(AccessToken token)
        {
            return _storage.SetItem(TokenKey, token);
        }

        public async Task<bool> HasToken()
        {
            return await GetToken() != null;
        }
    }
}
