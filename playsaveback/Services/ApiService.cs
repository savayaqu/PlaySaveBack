using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using playsaveback.Properties;

namespace playsaveback.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            var baseUrl = "https://savayaqu.duckdns.org:444/playsaveback/";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            // Автоматическая загрузка токена из настроек
            LoadAuthToken();
        }

        private void LoadAuthToken()
        {
            var savedToken = Settings.Default.AuthToken;
            if (!string.IsNullOrEmpty(savedToken))
            {
                SetAuthToken(savedToken);
            }
        }

        public void SetAuthToken(string token)
        {
            Settings.Default.AuthToken = token; // Сохраняем в настройки
            Settings.Default.Save();

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(endpoint, jsonContent);
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<T>(responseData);
        }

        public async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(endpoint, jsonContent);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}
