using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SrcGen;

[Generator]
public class FormatListSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var src = new StringBuilder();

        src.AppendLine(
            """
            namespace BinFmtScan;

            internal struct FormatInfo
            {
                public string ID;
                public IDetector Detector;
            };

            internal static partial class FormatList
            {
                internal static List<FormatInfo> All =
                [
            """);

        // Retrieve the syntax receiver
        if (context.SyntaxReceiver is MySyntaxReceiver receiver)
        {
            // Get the compilation
            var compilation = context.Compilation;

            // Get the interface symbol
            var interfaceSymbol = compilation.GetTypeByMetadataName("BinFmtScan.IDetector");
            if (interfaceSymbol != null)
            {
                // Process each class declaration to check if it implements the specified interface
                foreach (var classDeclaration in receiver.CandidateClassDeclarations)
                {
                    var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                    // Get the symbol for the class declaration
                    var classSymbol = model.GetDeclaredSymbol(classDeclaration);

                    if (classSymbol is INamedTypeSymbol namedTypeSymbol)
                    {
                        if (namedTypeSymbol.AllInterfaces.Contains(interfaceSymbol))
                        {
                            var id = namedTypeSymbol.ToString().Replace("BinFmtScan.", "");
                            var detector_class = namedTypeSymbol.ToString().Replace("BinFmtScan.", "");

                            src.AppendLine(
                                $$"""
                                      new FormatInfo
                                      {
                                          ID = "{{id}}",
                                          Detector = new {{detector_class}}(),
                                      },
                                """);
                        }
                    }
                }
            }
        }

        src.AppendLine(
            """
                ];
            }
            """);

        context.AddSource("FileFormats.cs", src.ToString());
    }

    // Syntax receiver to collect class declarations
    private class MySyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClassDeclarations { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                CandidateClassDeclarations.Add(classDeclaration);
            }
        }
    }
}