// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.GeneratedCodeAnalysis;

#if !ROSLYN_4_4_0_OR_GREATER
#pragma warning disable RS1012 // Start action has no registered actions
#endif

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute
#pragma warning disable RS1022 // Remove access to our implementation types 
#pragma warning disable RS1025 // Configure generated code analysis
#pragma warning disable RS1026 // Enable concurrent execution

internal sealed class CompileTimeGeneratedCodeAnalyzer : GeneratedCodeAnalyzerBase
{
    public override void Initialize( AnalysisContext context )
    {
        base.Initialize( context );

        context.RegisterCompilationStartAction( this.AnalyzeCompilation );
    }

    private void AnalyzeCompilation( CompilationStartAnalysisContext context )
    {
#if ROSLYN_4_4_0_OR_GREATER // Roslyn 4.0.1 doesn't have IsGeneratedCode, so just do nothing in that case.
        var generatedTrees = new List<SyntaxTree>();

        var originalCompilation = OriginalCompilationInfo.OriginalCompilation;
        var originalConfiguration = OriginalCompilationInfo.OriginalConfiguration;

        Invariant.Assert( originalCompilation == null || originalCompilation.Assembly.Identity == context.Compilation.Assembly.Identity );

        context.RegisterSyntaxTreeAction(
            treeContext =>
            {
                // It's not going to be exactly the same as files produced by source generators, but I can't think of a better way to do this.
                if ( treeContext.IsGeneratedCode )
                {
                    generatedTrees.Add( treeContext.Tree );
                }
            } );

        context.RegisterCompilationEndAction(
            compilationEndContext =>
            {
                var transformedCompilation = compilationEndContext.Compilation;

                var pipeline = this.CreatePipeline( originalCompilation ?? transformedCompilation, context.Options );

                var configuration = originalConfiguration ?? this.TryInitializePipeline( pipeline, transformedCompilation, context.CancellationToken );

                if ( configuration != null )
                {
                    this.AnalyzeTrees( pipeline, configuration, compilationEndContext.ReportDiagnostic, transformedCompilation, generatedTrees, context.CancellationToken );
                }
            } );
#endif
    }
}