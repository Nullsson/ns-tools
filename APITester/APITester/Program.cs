using System.CommandLine;

namespace APITester;

internal class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = BuildRootCommand();
        await rootCommand.Parse(args).InvokeAsync();
    }

    private static RootCommand BuildRootCommand()
    {
        var usersOption = new Option<FileInfo>("--user-list", "-u")
        {
            Description = "Path to file with list of user credentials.",
            Required = true,
        };

        var routesOption = new Option<FileInfo>("--targets", "-t")
        {
            Description = "Paths to file with list of targets to test.",
            Required = true,
        };
        
        var rootCommand = new RootCommand("API Testing Tool");
        rootCommand.Add(usersOption);
        rootCommand.Add(routesOption);
        
        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var userlistPath = parseResult.GetValue(usersOption);
            var routesPath = parseResult.GetValue(routesOption);
            
            return await ExecuteApiTestAsync(userlistPath, routesPath);
        });
        
        return rootCommand;
    }

    private static async Task<int> ExecuteApiTestAsync(FileInfo userlistPath, FileInfo routesPath)
    {
        var OnlyTestSameInstance = true;
        
        var users = await User.ReadUsersFromFile(userlistPath);
        var targets = await TestTarget.ReadTargetsFromFile(routesPath);

        using var writer = new StreamWriter(ReportTimestampTitle());
        writer.WriteLine("Instance,User,Email,Route,Method,ResponseCode,Response,CURL String");
        
        foreach (var target in targets)
        {
            foreach (var user in users.Where(u => !OnlyTestSameInstance || u.Instance == target.Instance))
            {
                var tester = new APITester();
                tester.SetBasicAuth(user);
                
                Console.WriteLine($"\nExecuting: {target.Instance} {target.HTTPMethod} {target.Route} | User: {user.Instance} {user.Role} {user.Username}");
                
                var response = await tester.ExecuteRequest(target);
                
                // You can also access individual properties
                if (response.IsSuccess)
                {
                    Console.WriteLine($"✓ Request succeeded in {response.StatusCodeNumber}");
                }
                else
                {
                    Console.WriteLine($"✗ Request failed with status {response.StatusCodeNumber}");
                }
                
                await writer.WriteLineAsync(
                    $"{EscapeCsv(user.Instance)},{EscapeCsv(user.Role)},{EscapeCsv(user.Username)},{EscapeCsv(target.Route)},{EscapeCsv(target.HTTPMethod)},{response.StatusCodeNumber},{EscapeCsv(response.Content)},{EscapeCsv(await tester.GetCurlString())}");
            }
        }
        
        Console.WriteLine("Application Completed all tests.");
        return 0;
    }

    private static string ReportTimestampTitle()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileNme = $"APITestReport_{timestamp}.csv";
        return fileNme;
    }

    static string EscapeCsv(string value)
    {
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}