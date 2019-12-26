using System.Threading.Tasks;
using BlazorStorage.Interfaces;

namespace Ether.Types
{
    public class TokenService
    {
        private const string TokenKey = "___EtherToken___";
        private readonly ILocalStorage _storage;

        public TokenService(ILocalStorage storage)
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
