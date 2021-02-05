using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class TemplateAnalyzer : DiagnosticAnalyzer
    {

        static TemplateAnalyzer()
        {
            CompilerServiceProvider.Initialize();
        }

        public TemplateAnalyzer()
        {
            this.SupportedDiagnostics = DiagnosticHelper.GetDiagnosticDescriptors( typeof( GeneralDiagnosticDescriptors ) )
                .ToImmutableArray();
        }


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            
            

        public override void Initialize( AnalysisContext context )
        {
            // Don't analyze generated code, just user code.
            context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );

            // Don't enable concurrent execution. It does not make sense just for templates.
            context.EnableConcurrentExecution();

        }

      
    }
}
