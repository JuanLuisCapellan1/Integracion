namespace IntegracionBanco.Services
{
    public class CoreApiService
    {
        private readonly HttpClient _httpClient;

        public CoreApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> ForwardRequestToCoreApiAsync(HttpRequestMessage request)
        {
            return await _httpClient.SendAsync(request);
        }
    }
}
