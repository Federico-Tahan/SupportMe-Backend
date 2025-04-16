using Azure;
using Newtonsoft.Json;
using System.Text;
using static Google.Apis.Requests.BatchRequest;

namespace SupportMe.Helpers
{
    public static class HttpClientHelper
    {
        public static async Task<dynamic> GetAsync(string url, int timeOut = 5000, IDictionary<string, string> headers = null)
        {
            using (var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeOut) })
            {
                try
                {
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        }
                    }
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        public static async Task<dynamic> PostAsync(string url, dynamic body = null, int timeOut = 50000, IDictionary<string, string> headers = null)
        {
            using (var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeOut) })
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }

                if (body != null)
                {
                    string jsonBody = JsonConvert.SerializeObject(body);
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                try
                {
                    var response = await client.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Error {response.StatusCode}: {content}");
                    }

                    return content;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request failed: {ex.Message}");
                    throw new HttpRequestException(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    throw new HttpRequestException(ex.Message);
                }
            }
        }
    }
}
