using System.CommandLine;
using System.Text.Json;
using System.Threading.RateLimiting;
using FetchPrograms.Model.Intigriti;

namespace FetchPrograms;

internal class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = BuildRootCommand();
        await rootCommand.Parse(args).InvokeAsync();
    }

    private static RootCommand BuildRootCommand()
    {
        var intigritiOption = new Option<string>("--intigriti-api-key")
        {
            Description = "Intigriti api key, when provided programs will be fetched from intigriti.",
            Required = false
        };

        var rootCommand = new RootCommand("Fetch programs");
        rootCommand.Add(intigritiOption);
        
        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var intigritiAPIKey = parseResult.GetValue(intigritiOption);
            if (!string.IsNullOrEmpty(intigritiAPIKey))
            {
                Console.WriteLine("Fetching programs from intigriti...");
                
                using var intigritiClient = new IntigritiAPIClient(intigritiAPIKey);
                try
                {
                    var programs = await intigritiClient.GetProgramsAsync();

                    var semaphore = new SemaphoreSlim(2);                 // max 2 concurrent
                    var rateDelay = TimeSpan.FromMilliseconds(1000);       // ≈ 1.6 req/sec (safe)
                    
                    var tasks = programs.Records.Select(async p =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await Task.Delay(rateDelay);
                            return await intigritiClient.GetProgramDetailAsync(p.Id);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });
                    
                    var results = await Task.WhenAll(tasks);
                    
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var fileName = $"IntigritiPrograms_{timestamp}.json";
                    var programsJson = JsonSerializer.Serialize(results.Where(r => true), new JsonSerializerOptions { WriteIndented = true });
                    
                    File.WriteAllText(fileName, programsJson);
                }
                catch (HttpRequestException e)
                {
                    Console.Error.WriteLine($"Error fetching data: {e.Message}");
                    return 1;
                }
            }

            return 0;
        });

        return rootCommand;
    }
}