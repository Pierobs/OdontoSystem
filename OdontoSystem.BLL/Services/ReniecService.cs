using Newtonsoft.Json;
using OdontoSystem.Entities;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OdontoSystem.BLL.Services
{
    public class ReniecService
    {
        private readonly string _token;
        private static readonly HttpClient _client = new HttpClient();

        public ReniecService()
        {
            _token = ConfigurationManager.AppSettings["ApisNetPe:Token"];
        }

        public async Task<ReniecResponseDto> ConsultarDniAsync(string dni)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            var url = $"https://api.decolecta.com/v1/reniec/dni?numero={dni}";
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ReniecResponseDto>(json);
        }
    }
}