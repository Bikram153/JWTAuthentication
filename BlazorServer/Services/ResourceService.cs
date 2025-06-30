namespace BlazorServer.Services
{
    public class ResourceService
    {
        private readonly APIService aPIService;

        public ResourceService(APIService aPIService)
        {
            this.aPIService = aPIService;
        }

        public async Task<bool> Verify()
        {
            var result = await aPIService.GetAsync("resource/verify");
            return result.IsSuccessStatusCode;
        }
    }
}
