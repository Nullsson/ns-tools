using System.CommandLine;
using System.Text.Json;
using System.Threading.RateLimiting;
using FetchPrograms.Model;
using FetchPrograms.Model.Intigriti;

namespace FetchPrograms;

internal class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = BuildRootCommand();
        await rootCommand.Parse(args).InvokeAsync();
    }

    public static async Task<T> GetWithRetry<T>(Func<Task<T>> apiCall, Guid id)
    {
        var failCount = 0;
        while (true)
        {
            if (failCount > 3)
            {
                Console.WriteLine($"Program with id: {id}. Is cursed so we move on.");
                return default(T);
            }

            try
            {
                return await apiCall();
            }
            catch (HttpRequestException ex)
            {
                failCount++;
                Console.WriteLine($"Temporary failure for {id}: {ex.Message}");
                await Task.Delay(5000);
            }
        }
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
                    
                    var results = new List<IntigritiProgram>();
                    
                    foreach (var p in programs.Records)
                    {
                        var intigritiProgram = new IntigritiProgram();
                        intigritiProgram.Overview = p;
                        
                        intigritiProgram.Detail = await GetWithRetry(() => intigritiClient.GetProgramDetailAsync(p.Id), p.Id);
                        await Task.Delay(5000);
                        
                        
                        results.Add(intigritiProgram);
                    }
                    
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