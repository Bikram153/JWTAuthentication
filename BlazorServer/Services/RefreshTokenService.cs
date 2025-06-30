using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace BlazorServer.Services
{
    public class RefreshTokenService
    {
        private readonly ProtectedLocalStorage protectedLocalStorage;
        private readonly string key = "refresh_token";

        public RefreshTokenService(ProtectedLocalStorage protectedLocalStorage)
        {
            this.protectedLocalStorage = protectedLocalStorage;
        }

        public async Task Set(string value)
        {
            await protectedLocalStorage.SetAsync(key, value);
        }

        public async Task<string> Get()
        {
            var result = await protectedLocalStorage.GetAsync<string>(key);
            if (result.Success)
            {
                return result.Value;
            }

            return string.Empty;
        }

        internal async Task Remove()
        {
            await protectedLocalStorage.DeleteAsync(key);
        }
    }
}
