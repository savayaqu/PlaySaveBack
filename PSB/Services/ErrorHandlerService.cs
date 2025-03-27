using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace PSB.Services
{
    public static class ErrorHandlerService
    {
        public static Dictionary<string, string> ParseValidationErrors(string responseBody)
        {
            var errors = new Dictionary<string, string>();

            try
            {
                using var jsonDoc = JsonDocument.Parse(responseBody);

                if (jsonDoc.RootElement.TryGetProperty("errors", out var errorsElement))
                {
                    foreach (var error in errorsElement.EnumerateObject())
                    {
                        if (error.Value.GetArrayLength() > 0)
                        {
                            errors[error.Name] = error.Value[0].GetString() ?? $"Invalid {error.Name}";
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"JSON parsing error: {ex.Message}");
                errors["general"] = "Failed to parse server response";
            }

            return errors;
        }

        public static string GetNetworkError(HttpRequestException ex)
        {
            return ex.StatusCode switch
            {
                HttpStatusCode.NotFound => "Service not found",
                HttpStatusCode.Unauthorized => "Unauthorized access",
                _ => $"Network error: {ex.Message}"
            };
        }
    }
}
