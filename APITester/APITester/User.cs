namespace APITester;

public class User
{
    private const int InstanceIndex = 0;
    private const int RoleIndex = 1;
    private const int UsernameIndex = 2;
    private const int PasswordIndex = 3;

    
    public string Username { get; set; }
    public string Password { get; set; }
    
    public string Instance { get; set; }
    public string Role { get; set; }

    public static async Task<List<User>> ReadUsersFromFile(FileInfo usersFile)
    {
        var lines = await File.ReadAllLinesAsync(usersFile.FullName);
        return lines
            .Select(line => line.Split(" "))
            .Where(line => line.Length == 4)
            .Select(line =>
                new User
                {
                    Username = line[UsernameIndex],
                    Password = line[PasswordIndex],
                    Instance = line[InstanceIndex],
                    Role = line[RoleIndex],
                })
            .ToList();
    }
}