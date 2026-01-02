using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace APITester;

public class APITester
{
    private readonly HttpClient _client;
    private TestTarget? _lastTarget;
    private User? _lastUser;

    public APITester()
    {
        _client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(30);
    }

    public void SetBasicAuth(User user)
    {
        _lastUser = user;
        
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{user.Username}:{user.Password}")
        );
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<APIResponse> ExecuteRequest(TestTarget target)
    {
        _lastTarget = target;
        
        var stopwatch = Stopwatch.StartNew();
        var response = new APIResponse
        {
            Headers = new Dictionary<string, string>()
        };

        try
        {
            var method = new HttpMethod(target.HTTPMethod.ToUpper());
            var request = new HttpRequestMessage(method, target.Route);

            if (!string.IsNullOrEmpty(target.PayloadPath))
            {
                var jsonContent = await File.ReadAllTextAsync(target.PayloadPath);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            var httpResponse = await _client.SendAsync(request);
            stopwatch.Stop();

            response.StatusCode = httpResponse.StatusCode;
            response.IsSuccess = httpResponse.IsSuccessStatusCode;
            response.ReasonPhrase = httpResponse.ReasonPhrase ?? string.Empty;
            response.Duration = stopwatch.Elapsed;
            response.Content = await httpResponse.Content.ReadAsStringAsync();

            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.Error = ex;
            response.IsSuccess = false;
            response.Duration = stopwatch.Elapsed;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.Content = string.Empty;
        }

        return response;
    }
    
    public async Task<string> GetCurlString()
    {
        if (_lastTarget == null)
            throw new InvalidOperationException("No request has been executed yet. Call ExecuteRequest first.");

        var sb = new StringBuilder();
        sb.Append("curl");

        if (_lastTarget.HTTPMethod.ToUpper() != "GET")
        {
            sb.Append($" -X {_lastTarget.HTTPMethod.ToUpper()}");
        }

        sb.Append($" \"{_lastTarget.Route}\"");
        
        if (_lastUser != null)
        {
            sb.Append($" -u \"{_lastUser.Username}:{_lastUser.Password}\"");
        }
        
        if (!string.IsNullOrEmpty(_lastTarget.PayloadPath))
        {
            var jsonContent = await File.ReadAllTextAsync(_lastTarget.PayloadPath);
            var escapedJson = jsonContent
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "")
                .Replace("\r", "");
            
            sb.Append($" -H \"Content-Type: application/json\"");
            sb.Append($" -d \"{escapedJson}\"");
        }

        return sb.ToString();
    }
}