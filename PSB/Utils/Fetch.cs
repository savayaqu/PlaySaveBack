using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PSB.Api;
using PSB.Api.Response;
using PSB.Converters;

namespace PSB.Utils
{
    public static class Fetch
    {
        private readonly static HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(20) };

        public static async Task<HttpResponseMessage> FetchAsync(
            HttpMethod method,
            dynamic path,
            dynamic? body = null,
            bool serialize = false,
            CancellationToken cancellationToken = default
        )
        {
            Debug.WriteLine("FETCH: Start");


            // Готовим запрос
            Uri fullUrl;
            if (path is Uri uri) fullUrl = uri;
            else if (path is string urlEnd)
            {
                try
                {
                    fullUrl = new Uri($"{URLs.API_URL}/{urlEnd}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}");
                    return new();
                }
            }
            else throw new ArgumentException("path should be of the type: Uri, string");

            Debug.WriteLine("FETCH: FullUrl: " + fullUrl);

            var request = new HttpRequestMessage(method, fullUrl);
            if (AuthData.Token != null)
                request.Headers.Add("Authorization", $"Bearer {AuthData.Token}");

            if (body != null)
            {
                if (serialize)
                {
                    string json = JsonSerializer.Serialize(body);
                    Debug.WriteLine("FETCH: REQUEST: JSON: " + json);
                    request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                }
                else if (body is HttpContent)
                {
                    request.Content = body;
                }
                else throw new ArgumentException("body should be of the type HttpContent if serialize is false");
            }

            try
            {
                // Запрашиваем
                var response = await _httpClient.SendAsync(request, cancellationToken);
                
                // Обрабатываем ответ
                if (!response.IsSuccessStatusCode)
                {
                    var responseJsonErr = await response.Content.ReadAsStringAsync(cancellationToken);
                    try
                    {
                        Debug.WriteLine("FETCH: ERROR: Json: " + responseJsonErr);
                        var responseBodyErr = JsonSerializer.Deserialize<ErrorResponse>(responseJsonErr);
                        if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            if (responseBodyErr.Message == "Unauthorized")
                            {
                                //TODO: вместо диалога выводить справа снизу infobar
                                var dialog =  App.DialogService!.ShowConfirmationAsync("Ошибка", "Перезайдите в аккаунт");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Fetcher: responseJsonErr: " + ex.Message);
                    }
                }
                return response;
            }
            catch (TaskCanceledException)
            {
                return new();
            }
            catch (HttpRequestException ex)
            {
                string message = ex.Message == "Connection failure" ? "Не удалось установить соединение с сервером" : ex.Message;
                return new();
            }
        }

        public static async Task<(HttpResponseMessage, T?)> FetchAsync<T>(
            HttpMethod method,
            dynamic path,
            dynamic? body = null,
            bool serialize = false,
            CancellationToken cancellationToken = default
            )
        {
            HttpResponseMessage response = await FetchAsync(method, path, body, serialize, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return (response, default(T));

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine("FETCH: responseJson: " + responseJson);

                // Проверяем, что ответ не пустой
                if (string.IsNullOrWhiteSpace(responseJson))
                {
                    Debug.WriteLine("FETCH: Empty response received");
                    return (response, default(T));
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new NullableDateTimeConverter() }
                };

                // Дополнительная проверка на валидность JSON
                if (!IsValidJson(responseJson))
                {
                    Debug.WriteLine("FETCH: Invalid JSON received");
                    return (response, default(T));
                }

                var responseBody = JsonSerializer.Deserialize<T>(responseJson, options);
                Debug.WriteLine("FETCH: responseBody: " + responseBody);
                return (response, responseBody);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error during deserialization: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (response, default(T));
            }
        }

        // Проверка, что строка является валидным JSON
        private static bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

