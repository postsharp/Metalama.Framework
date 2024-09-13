// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;

namespace Metalama.Framework.Engine.GeneratedCodeAnalysis;

#if !ROSLYN_4_4_0_OR_GREATER
#pragma warning disable RS1012 // Start action has no registered actions
#endif

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute
#pragma warning disable RS1022 // Remove access to our implementation types 
#pragma warning disable RS1025 // Configure generated code analysis
#pragma warning disable RS1026 // Enable concurrent execution

internal sealed class DesignTimeGeneratedCodeAnalyzer : GeneratedCodeAnalyzerBase
{
    public override void Initialize( AnalysisContext context )
    {
        base.Initialize( context );

        context.RegisterCompilationStartAction( this.AnalyzeCompilation );
    }

    private void AnalyzeCompilation( CompilationStartAnalysisContext context )
    {
#if ROSLYN_4_4_0_OR_GREATER // Roslyn 4.0.1 doesn't have IsGeneratedCode, so just do nothing in that case.
        var compilation = context.Compilation;

        var pipeline = this.CreatePipeline( compilation, context.Options );

        if ( this.TryInitializePipeline( pipeline, compilation, context.CancellationToken ) is { } configuration )
        {
            context.RegisterSyntaxTreeAction(
                treeContext =>
                {
                    // It's not going to be exactly the same as files produced by source generators, but I can't think of a better way to do this.
                    if ( !treeContext.IsGeneratedCode )
                    {
                        return;
                    }

                    this.AnalyzeTrees( pipeline, configuration, treeContext.ReportDiagnostic, compilation, [treeContext.Tree], treeContext.CancellationToken );
                } );
        }
#endif
    }
}