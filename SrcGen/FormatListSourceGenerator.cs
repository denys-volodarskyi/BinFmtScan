using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

namespace SrcGen;

[Generator]
public class FormatListSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext ctx)
    {
        var src = new StringBuilder();

        var syms = ctx.Compilation.GetSymbolsWithName(predicate: x => true, cancellationToken: ctx.CancellationToken);
        var fmts = syms.Where(s => s.Kind == SymbolKind.NamedType && s.ToString().Contains(".Formats."));

        src.AppendLine(
            """
            namespace BinFmtScan;

            internal static class FormatList
            {
                internal static List<IDetector> All = new()
                {
            """);

        foreach (var fmt in fmts)
            src.AppendLine($"      new {fmt}(),");


        src.AppendLine(
        """
                };
            }
            """);

        ctx.AddSource("FileFormats.cs", src.ToString());
    }
}