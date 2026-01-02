namespace APITester;

public class TestTarget
{
    private const int InstanceIndex = 0;
    private const int HTTPMethodIndex = 1;
    private const int RouteIndex = 2;
    private const int PayloadIndex = 3;
    
    public string Instance { get; set; }
    public string HTTPMethod { get; set; }
    public string Route { get; set; }
    public string? PayloadPath { get; set; }
    
    public static async Task<List<TestTarget>> ReadTargetsFromFile(FileInfo targetsFile)
    {
        var lines = await File.ReadAllLinesAsync(targetsFile.FullName);
        return lines
            .Where(l => !l.StartsWith("#"))
            .Where(l => !string.IsNullOrEmpty(l))
            .Select(line => line.Split(" "))
            .Where(line => line.Length >= 3)
            .Select(line =>
            {
                var TestTarget = new TestTarget();

                TestTarget.Instance = line[InstanceIndex];
                TestTarget.HTTPMethod = line[HTTPMethodIndex];
                TestTarget.Route = line[RouteIndex];

                if (line.Length > 3)
                {
                    TestTarget.PayloadPath = line[PayloadIndex];
                }
                
                return TestTarget;
            })
            .ToList();
    }
}