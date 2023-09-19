using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeAnalysis.Rules
{
    public class NullableCastToNonNullableDetector
    {
        // int? b = (int)std.Age;
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

                foreach (var doc in project.Documents)
                {
                    if (excludeFileName.Any(x => doc.Name.Contains(x)))
                        continue;

                    var tree = await doc.GetSyntaxTreeAsync();
                    var root = await tree.GetRootAsync();
                    var semanticModel = await doc.GetSemanticModelAsync();

                    var castExpressionSyntaxes = root.DescendantNodesAndSelf().OfType<AssignmentExpressionSyntax>()
                        .Where(x => x.Kind().Equals(SyntaxKind.SimpleAssignmentExpression))
                        .SelectMany(x => x.ChildNodes().OfType<CastExpressionSyntax>())
                        .Where(x => x.Type.Kind().Equals(SyntaxKind.PredefinedType))
                        .ToArray();

                    foreach (var castExpression in castExpressionSyntaxes)
                    {
                        var expression = castExpression.Expression;
                        if (expression is null) continue;

                        var lastChild = expression.ChildNodes().LastOrDefault();
                        if (lastChild == null) continue;

                        var typeInfo = semanticModel.GetTypeInfo(lastChild).Type as INamedTypeSymbol;
                        if (typeInfo != null && typeInfo.Name.Equals("Nullable"))
                        {
                            violations.Add($"Found nullable cast to non-nullable: {castExpression.Parent} at {castExpression.GetLocation().GetLineSpan()}");
                        }
                    }
                }
            }
            return violations.ToArray();
        }
    }
}