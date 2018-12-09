using System;

namespace Ether.Types
{
    public class AccessToken
    {
        public AccessToken()
        {
        }

        public AccessToken(string token, TimeSpan expiresIn)
        {
            Token = token;
            ExpiresAt = DateTime.UtcNow.Add(expiresIn);
        }

        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
