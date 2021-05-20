﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.DesignTime.Utilities;
using Caravela.Framework.Impl.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

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

            try
            {
                DesignTimeLogger.Instance?.Write( $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}')." );

                var buildOptions = new ProjectOptions( context.AnalyzerConfigOptions );

                DesignTimeDebugger.AttachDebugger( buildOptions );

                // Execute the pipeline.
                var results = DesignTimeAspectPipelineCache.Instance.GetSyntaxTreeResults(
                    compilation,
                    buildOptions,
                    context.CancellationToken );

                // Add introduced syntax trees.
                var sourcesCount = 0;

                foreach ( var syntaxTreeResult in results )
                {
                    if ( syntaxTreeResult != null )
                    {
                        foreach ( var additionalSyntaxTree in syntaxTreeResult.Introductions )
                        {
                            sourcesCount++;
                            context.AddSource( additionalSyntaxTree.Name, additionalSyntaxTree.GeneratedSyntaxTree.GetText() );
                        }
                    }
                }

                DesignTimeLogger.Instance?.Write( $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}'): {sourcesCount} sources generated." );

                // We don't report diagnostics because it seems to be without effect.
                // All diagnostics are reported by the analyzer.
            }
            catch ( Exception e )
            {
                DesignTimeLogger.Instance?.Write( e.ToString() );
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
            this._isEnabled = !CaravelaCompilerInfo.IsActive;
        }
    }
}