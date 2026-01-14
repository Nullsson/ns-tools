using System.CommandLine;

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
                    var data = await intigritiClient.GetProgramsAsync();
                    Console.WriteLine(data);
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