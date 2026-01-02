using System.CommandLine;
using System.Reflection.Metadata;

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
        var users = await User.ReadUsersFromFile(userlistPath);
        
        
        
        return 1;
    }
}