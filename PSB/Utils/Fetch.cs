using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        private readonly static HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

        public static async Task<HttpResponseMessage> FetchAsync(
            HttpMethod method,
            dynamic path,
            Action<bool>? setIsFetch = null,
            Action<string>? setError = null,
            dynamic? body = null,
            bool serialize = false,
            CancellationToken cancellationToken = default
        )
        {
            Debug.WriteLine("FETCH: Start");
            setIsFetch?.Invoke(true);
            setError?.Invoke(null);

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
                    setIsFetch?.Invoke(false);
                    setError?.Invoke($"Ошибка URL: {ex.Message}");
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
                        setError?.Invoke(responseBodyErr?.Message);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Fetcher: responseJsonErr: " + ex.Message);
                        setError?.Invoke($"Пришёл плохой ответ {(int)response.StatusCode} ({response.ReasonPhrase})");
                        //setError?.Invoke("Не удалось прочитать ошибку"); // TODO: FIXME: удалить setError
                    }
                }
                setIsFetch?.Invoke(false);
                return response;
            }
            catch (TaskCanceledException)
            {
                setIsFetch?.Invoke(false);
                setError?.Invoke("Время ожидания вышло");
                return new();
            }
            catch (HttpRequestException ex)
            {
                setIsFetch?.Invoke(false);
                string message = ex.Message == "Connection failure" ? "Не удалось установить соединение с сервером" : ex.Message;
                setError?.Invoke(message);
                return new();
            }
        }

        public static async Task<(HttpResponseMessage, T?)> FetchAsync<T>(
            HttpMethod method,
            dynamic path,
            Action<bool>? setIsFetch = null,
            Action<string>? setError = null,
            dynamic? body = null,
            bool serialize = false,
            CancellationToken cancellationToken = default
        )
        {
            HttpResponseMessage response = await FetchAsync(method, path, setIsFetch, setError, body, serialize, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return (response, default(T));

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                Debug.WriteLine("FETCH: responseJson: " + responseJson);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Игнорируем регистр имён свойств
                    Converters = { new NullableDateTimeConverter() } // Добавляем конвертер
                };

                var responseBody = JsonSerializer.Deserialize<T>(responseJson, options);
                Debug.WriteLine("FETCH: responseBody: " + responseBody);
                return (response, responseBody);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error during deserialization: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }

        }
    }
}

