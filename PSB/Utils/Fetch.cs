using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PSB.Api;
using PSB.Api.Response;
using PSB.Converters;
using PSB.Services;

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
            if (path is Uri uri)
            {
                fullUrl = uri;
            }
            else if (path is string urlEnd)
            {
                try
                {
                    // Проверяем, начинается ли строка с http:// или https://
                    if (urlEnd.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        urlEnd.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        fullUrl = new Uri(urlEnd);
                    }
                    else
                    {
                        fullUrl = new Uri($"{URLs.API_URL}/{urlEnd}");
                    }
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
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    Debug.WriteLine($"FETCH: ERROR: Status: {response.StatusCode}, Content: {responseContent}");

                    // Проверяем, является ли ответ JSON
                    if (response.Content.Headers.ContentType?.MediaType?.Contains("application/json") == true)
                    {
                        try
                        {
                            var responseBodyErr = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                if (responseBodyErr?.Message == "Unauthorized")
                                {
                                    NotificationService.ShowError("Перезайдите в аккаунт");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Fetcher: Error parsing error response: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Если это HTML, попробуем извлечь полезную информацию
                        string errorMessage = ExtractErrorMessageFromHtml(responseContent);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            Debug.WriteLine($"FETCH: Extracted error message: {errorMessage}");
                            NotificationService.ShowError(errorMessage);
                        }
                        else
                        {
                            Debug.WriteLine($"FETCH: Received non-JSON error response");
                            NotificationService.ShowError($"Ошибка сервера: {response.StatusCode}");
                        }
                    }
                }
                return response;
            }
            catch (TaskCanceledException)
            {
                NotificationService.ShowError("Превышено время ожидания ответа от сервера");
                return new();
            }
            catch (HttpRequestException ex)
            {
                string message = ex.Message == "Connection failure" ? "Не удалось установить соединение с сервером" : ex.Message;
                NotificationService.ShowError(message);
                return new();
            }
        }

        private static string ExtractErrorMessageFromHtml(string htmlContent)
        {
            // Простая попытка извлечь сообщение об ошибке из HTML
            // Может потребоваться доработка под конкретный формат ошибок Laravel

            // Ищем типичные паттерны ошибок Laravel
            int startIndex = htmlContent.IndexOf("<div class=\"error-message\">") + "<div class=\"error-message\">".Length;
            if (startIndex < 0)
            {
                startIndex = htmlContent.IndexOf("<h1 class=\"text-red-600\">") + "<h1 class=\"text-red-600\">".Length;
            }

            if (startIndex >= 0)
            {
                int endIndex = htmlContent.IndexOf("</", startIndex);
                if (endIndex > startIndex)
                {
                    return htmlContent.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }

            // Если не нашли по паттернам, вернем первые 200 символов или пустую строку
            return htmlContent.Length > 200 ? htmlContent.Substring(0, 200) : htmlContent;
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
    }
}