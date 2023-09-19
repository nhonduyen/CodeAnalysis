// See https://aka.ms/new-console-template for more information
using CodeAnalysis;
using Microsoft.Build.Locator;
using Microsoft.Extensions.Configuration;

RegisterVisualStudioInstance();
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var configArgs = config.GetSection("Args").Value;
var ignoreRuleString = config.GetSection("IgnoreRules").Value;

Console.WriteLine(configArgs);

var hasViolation = await AnalyzeAndReportAsync(configArgs);

Console.WriteLine("Press any key to exit");
Console.ReadLine();

Environment.Exit(hasViolation ? 1 : 0);

static void RegisterVisualStudioInstance()
{
    var vsInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
    if (vsInstances.Length <= 0)
    {
        throw new Exception("Visual Studio is required");
    }
    var instance = vsInstances.OrderByDescending(x => x.Version).FirstOrDefault();
    MSBuildLocator.RegisterInstance(instance);
    Console.WriteLine($"Using MSbuild at {instance.MSBuildPath} to load projects");
}

static async Task<bool> AnalyzeAndReportAsync(string solutionQueryPattern)
{
    var violationReporter = new ViolationReporter();
    return await violationReporter.ReportAsync(solutionQueryPattern);
}