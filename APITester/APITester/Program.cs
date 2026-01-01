using System.CommandLine;

namespace APITester;

internal class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = BuildRootCommand();
    }

    private static RootCommand BuildRootCommand()
    {
        var usersOption = new Option<string>("--user-list", "-u")
        {
            Description = "Path to file with list of user credentials.",
            Required = true,
        };

        var routesOption = new Option<string>("--routes", "-r")
        {
            Description = "Paths to file with list of routes to test.",
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

    private static async Task<int> ExecuteApiTestAsync(string userlistPath, string routesPath)
    {
        return 1;
    }
}