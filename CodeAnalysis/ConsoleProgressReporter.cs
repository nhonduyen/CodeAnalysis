using Microsoft.CodeAnalysis.MSBuild;

namespace CodeAnalysis
{
    public class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
    {
        public void Report(ProjectLoadProgress loadProgress)
        {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (projectDisplay != null)
            {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }
            Console.WriteLine($"{loadProgress.Operation,15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }
}
