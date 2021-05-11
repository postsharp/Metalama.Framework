﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="ISourceGenerator"/>. Provides the source code generated by the pipeline.
    /// </summary>
    [Generator]
    public class DesignTimeSourceGenerator : ISourceGenerator
    {
        private bool _isEnabled;

        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( !this._isEnabled || context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }
            
            var buildOptions = new BuildOptions( context.AnalyzerConfigOptions );
            
            DesignTimeDebugger.AttachDebugger( buildOptions );

            // Execute the pipeline.
            var results = DesignTimeAspectPipelineCache.Instance.GetDesignTimeResults(
                compilation,
                buildOptions,
                context.CancellationToken );

            // Add introduced syntax trees.
            foreach ( var syntaxTreeResult in results.SyntaxTreeResults )
            {
                if ( syntaxTreeResult != null )
                {
                    foreach ( var additionalSyntaxTree in syntaxTreeResult.Introductions )
                    {
                        context.AddSource( additionalSyntaxTree.Name, additionalSyntaxTree.GeneratedSyntaxTree.GetText() );
                    }
                }
            }

            // We don't report diagnostics because it seems to be without effect.
            // All diagnostics are reported by the analyzer.
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
            this._isEnabled = !CaravelaCompilerInfo.IsActive;
        }
    }
}