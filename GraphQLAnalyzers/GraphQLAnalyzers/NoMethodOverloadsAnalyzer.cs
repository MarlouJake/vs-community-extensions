using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace GraphQLAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoMethodOverloadsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GQLDA001NOL";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Method overload detected",
            messageFormat: "Method '{0}' has overloads in the same class",
            category: "Naming",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
            );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Analyze all named types (classes, structs, etc.)
            context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        }

        private void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var typeSymbol = (INamedTypeSymbol)context.Symbol;

            // Only analyze classes named "Query" or "Mutation"
            if (typeSymbol.TypeKind != TypeKind.Class)
                return;

            if (typeSymbol.Name != "Query" && typeSymbol.Name != "Mutation")
                return;

            // Get all methods (across all partial parts automatically)
            var methods = typeSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind != MethodKind.Constructor) // Ignore constructors
                .ToList();

            // Group by method name
            var groups = methods.GroupBy(m => m.Name);

            foreach (var group in groups)
            {
                if (group.Count() > 1)
                {
                    // Found overloads: report diagnostic on each method
                    foreach (var methodSymbol in group)
                    {
                        var diagnostic = Diagnostic.Create(
                            Rule,
                            methodSymbol.Locations.FirstOrDefault() ?? Location.None,
                            methodSymbol.Name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
