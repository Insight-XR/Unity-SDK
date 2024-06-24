using RecipeEngine;
using RecipeEngine.Modules.Wrench;
using RecipeEngine.Modules.Wrench.Helpers;

public static class WrenchJobsGenerator
{
    public static void Main(string[] args)
    {
        const string yamatoDirName = ".yamato";

        var repoRootDir = GitHelper.GetRepositoryRootDirectory();
        var repoYamatoDir = Path.Combine(repoRootDir, yamatoDirName);

        var result = EngineFactory
            .Create()
            .ScanAll()
            .GenerateWrenchJobs()
            .GenerateAsync().Result;

        if (result == 0)
            CopyLocalFilesToRoot(Path.Combine(Directory.GetCurrentDirectory(), yamatoDirName), repoYamatoDir);
        else
        {
            Console.WriteLine($"Exit code : {result}");
        }
    }

    static void CopyLocalFilesToRoot(string targetYamatoPath, string repoYamatoPath)
    {
        if (!Directory.Exists(repoYamatoPath))
        {
            Directory.CreateDirectory(repoYamatoPath);
        }

        Console.WriteLine($"Moving generated jobs from '{targetYamatoPath}' into '{repoYamatoPath}'...");
        foreach (var filePath in Directory.EnumerateFiles(targetYamatoPath, "*.yml"))
        {
            var newLocation = Path.Combine(repoYamatoPath, Path.GetFileName(filePath));

            File.Copy(filePath, newLocation, true);
        }
    }
}
