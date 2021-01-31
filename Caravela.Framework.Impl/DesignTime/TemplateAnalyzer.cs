using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime
{
    [DiagnosticAnalyzer( LanguageNames.CSharp )]
    public class TemplateAnalyzer : DiagnosticAnalyzer
    {

        static TemplateAnalyzer()
        {
            ProjectDesignTimeEntryPoint.Initialize();
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

            context.RegisterCodeBlockAction( this.AnalyzeCodeBlock );
            context.RegisterOperationBlockStartAction( this.OperationBlockStart );

        }

        private void Operation( OperationAnalysisContext context )
        {
            Analyze( context.Operation.SemanticModel, context.ReportDiagnostic, context.CancellationToken );
        }

        private void OperationBlockStart( OperationBlockStartAnalysisContext context )
        {
            context.RegisterOperationAction( this.Operation, OperationKind.Loop, OperationKind.VariableDeclarator, OperationKind.Conditional, OperationKind.MethodReference );
        }

        private void AnalyzeCodeBlock( CodeBlockAnalysisContext context )
        {
            Analyze( context.SemanticModel, context.ReportDiagnostic, context.CancellationToken );
        }

        private void AnalyzeSymbol( SymbolAnalysisContext context )
        {
            
        }

        private void AnalyzeSemanticModel( SemanticModelAnalysisContext context )
        {
            

        }

        private static void Analyze( SemanticModel semanticModel,  Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken )
        {
            if ( !ProjectDesignTimeEntryPoint.Instance.HasTemplateHighlightingUpdatedClient )
            {
                // Nobody is interested.
                return;
            }

            var diagnostics = new List<Diagnostic>();
            var templateCompiler = new TemplateCompiler();


            templateCompiler.TryAnnotate( semanticModel.SyntaxTree.GetRoot( cancellationToken ),
                semanticModel, diagnostics, out var annotatedSyntaxRoot );

            if ( annotatedSyntaxRoot != null )
            {
                foreach ( var diagnostic in diagnostics )
                {
                    reportDiagnostic( diagnostic );
                }

                var text = semanticModel.SyntaxTree.GetText( cancellationToken );
                CompileTimeTextSpanMarker marker = new CompileTimeTextSpanMarker( text );

                ProjectDesignTimeEntryPoint.Instance.SignalTemplateHighlightingUpdated( new TemplateHighlightingInfo( text, marker.Classifier ) );

            }
        }
    }
}
