using System.Net;
using System.Text;

namespace APITester;

public class APIResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public int StatusCodeNumber => (int)StatusCode;
    public bool IsSuccess { get; set; }
    public string Content { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string ReasonPhrase { get; set; }
    public Exception? Error { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Status: {StatusCodeNumber} {StatusCode}");
        sb.AppendLine($"Success: {IsSuccess}");
        sb.AppendLine($"Duration: {Duration.TotalMilliseconds:F2}ms");
        
        if (!string.IsNullOrEmpty(ReasonPhrase))
            sb.AppendLine($"Reason: {ReasonPhrase}");
        
        if (Error != null)
            sb.AppendLine($"Error: {Error.Message}");
        
        // if (!string.IsNullOrEmpty(Content))
        //     sb.AppendLine($"Content:\n{Content}");
        
        return sb.ToString();
    }
}