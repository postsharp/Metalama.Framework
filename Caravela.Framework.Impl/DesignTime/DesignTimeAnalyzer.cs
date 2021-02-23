using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// A fake analyzer that has the desired side effect of initializing <see cref="CompilerServiceProvider"/>.
    /// </summary>
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class DesignTimeAnalyzer : DiagnosticAnalyzer
    {

        static DesignTimeAnalyzer()
        {
            CompilerServiceProvider.Initialize();
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize( AnalysisContext analysisContext )
        {
        }
    }
}
