using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis.Rules
{
    public class SettableStaticFieldOrPropDetector
    {
        public async Task<string[]> DetectViolationAsync(Solution solution)
        {
            var violations = new List<string>();
            var excludeFileName = new List<string>()
            {
                ".AssemblyAttributes.cs",
                ".AssemblyInfo.cs",
                ".Designer.cs"
            };
            foreach (var id in solution.ProjectIds)
            {
                var project = solution.GetProject(id);
                if (project.Name.StartsWith("Test"))
                    continue;

                if (project.Name.EndsWith("UnitTest"))
                    continue;

                foreach (var doc in project.Documents)
                {
                    if (excludeFileName.Any(x => doc.Name.Contains(x)))
                        continue;

                    var tree = await doc.GetSyntaxTreeAsync();
                    var root = await tree.GetRootAsync();

                    var allSettableStaticFieldOrProp = root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>()
                        .Where(x => x.ChildNodes().Any(c => !c.GetLeadingTrivia().ToString().Contains("TODO: Fix SettableStaticFieldOrProp") &&
                        (
                        (c.IsKind(SyntaxKind.FieldDeclaration) && c.ChildTokens().Any(x => x.IsKind(SyntaxKind.StaticKeyword)) && !c.ChildTokens().Any(x => x.IsKind(SyntaxKind.ConstKeyword) || x.IsKind(SyntaxKind.ReadOnlyKeyword)))
                        ||
                        (c.IsKind(SyntaxKind.PropertyDeclaration) && c.ChildTokens().Any(x => x.IsKind(SyntaxKind.StaticKeyword)) && c.DescendantNodes().Any(x => x.IsKind(SyntaxKind.SetAccessorDeclaration)))
                        )
                        ))
                        .ToArray();

                    foreach (var violationItem in allSettableStaticFieldOrProp)
                    {
                        violations.Add($"Found SettableStaticFieldOrProp: {violationItem.Parent} at {violationItem.GetLocation().GetLineSpan()}");
                    }
                }
            }
            return violations.ToArray();
        }
    }
}
