namespace IntegracionBanco.Services
{
    public class CoreStatusService
    {
        private readonly HttpClient _httpClient;
        private readonly string _coreApiUrl;

        public CoreStatusService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _coreApiUrl = configuration.GetValue<string>("CoreApiUrl");
        }

        public async Task<bool> IsCoreApiActiveAsync()
        {
            try
            {
                Console.WriteLine(_coreApiUrl);
                HttpResponseMessage response = await _httpClient.GetAsync($"{_coreApiUrl}/WeatherForecast");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }
    }
}
