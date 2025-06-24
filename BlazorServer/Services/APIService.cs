using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace BlazorServer.Services
{
    public class APIService
    {
        private readonly AccessTokenService tokenService;
        private readonly AuthService authService;
        private readonly NavigationManager nav;
        private HttpClient client;

        public APIService(
            IHttpClientFactory httpClientFactory,
            AccessTokenService tokenService,
            AuthService authService,
            NavigationManager nav)
        {
            client = httpClientFactory.CreateClient("ApiClient");
            this.tokenService = tokenService;
            this.authService = authService;
            this.nav = nav;
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            var token = await tokenService.GetToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var responseMessage = await client.GetAsync(endpoint);
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                //call refersh token
                var refreshTokenResult = await authService.RefreshTokenAsync();
                if (!refreshTokenResult)
                {
                    await authService.Logout();
                }

                var newToken = await tokenService.GetToken();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                var newResponse = await client.GetAsync(endpoint);
                return newResponse;
            }

            return responseMessage;
        }

        public async Task<HttpResponseMessage> PostDataAsync(string endpoint, object obj)
        {
            var token = await tokenService.GetToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var responseMessage = await client.PostAsJsonAsync(endpoint, obj);
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                //call refresh token
                var refreshTokenResult = await authService.RefreshTokenAsync();
                if (!refreshTokenResult)
                {
                    await authService.Logout();
                }

                var newToken = await tokenService.GetToken();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                var newResponse = await client.PostAsJsonAsync(endpoint, obj);
                return newResponse;
            }
            return responseMessage;
        }
    }
}
