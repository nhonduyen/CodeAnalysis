using CodeAnalysis.Models;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using CodeAnalysis.Rules;

namespace CodeAnalysis
{
    public class ViolationReporter
    {
        public async Task<bool> ReportAsync(string slnQueryPattern)
        {
            bool hasViolation = false;
            var violations = new Dictionary<string, Dictionary<ViolationKind, string[]>>();
            var slnPaths = GetSolutionPaths(slnQueryPattern);
            foreach (var slnPath in slnPaths)
            {
                var violationReport = await DetectViolationsAsync(slnPath);
                var violation = violationReport.Data.ToDictionary(x => x.Key, x => x.Value);
                if (violation.Values.Any())
                {
                    violations.Add(slnPath, violation);
                }
            }

            if (violations.Count > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var violation in violations)
                {
                    if (violation.Value.Count > 0)
                    {
                        Console.WriteLine($"Violation in {violation.Key}");
                        foreach (var perRuleViolation in violation.Value)
                        {
                            Console.WriteLine($"\tRule: {perRuleViolation.Key}");
                            foreach (var viola in perRuleViolation.Value)
                            {
                                hasViolation = true;
                                Console.WriteLine($"\t\t - {viola}");
                            }
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            return hasViolation;
        }

        private async Task<ViolationReport> DetectViolationsAsync(string solutionPath)
        {
            var result = new ViolationReport();
            using var msWorkspace = MSBuildWorkspace.Create();
            msWorkspace.SkipUnrecognizedProjects = true;
            msWorkspace.WorkspaceFailed += (sender, args) =>
            {
                if (args.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                {
                    Console.Error.WriteLine(args.Diagnostic.Message);
                }
            };
            var solution = await msWorkspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading solution {solutionPath}");

            var nullableCastToNonNullable = new NullableCastToNonNullableDetector();
            result.Data.Add(ViolationKind.NullableCastToNonNullable, await nullableCastToNonNullable.DetectViolationAsync(solution));

            var settableStaticFieldOrProp = new SettableStaticFieldOrPropDetector();
            result.Data.Add(ViolationKind.SettableStaticFieldOrProp, await settableStaticFieldOrProp.DetectViolationAsync(solution));

            return result;
        }

        private string[] GetSolutionPaths(string slQueryPattern)
        {
            if (File.Exists(slQueryPattern))
            {
                return new string[] { slQueryPattern };
            }
            Matcher matcher = new Matcher();
            matcher.AddInclude(slQueryPattern);

            var result = matcher.Execute(
                new DirectoryInfoWrapper(new DirectoryInfo(Environment.CurrentDirectory))
                );
            return result.Files.Select(x => x.Path).ToArray();
        }

    }
}
