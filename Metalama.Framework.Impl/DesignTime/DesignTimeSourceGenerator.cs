﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Impl.AdditionalOutputs;
using Metalama.Framework.Impl.DesignTime.Pipeline;
using Metalama.Framework.Impl.Options;
using Metalama.Framework.Impl.Pipeline;
using Metalama.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Impl.DesignTime
{
    /// <summary>
    /// Our implementation of <see cref="ISourceGenerator"/>. Provides the source code generated by the pipeline.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
                Logger.Instance?.Write( $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}')." );

                var projectOptions = new ProjectOptions( context.AnalyzerConfigOptions );

                DebuggingHelper.AttachDebugger( projectOptions );

                if ( !projectOptions.IsDesignTimeEnabled )
                {
                    // Execute the fallback.
                    Logger.Instance?.Write(
                        $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}'): DesignTimeEnabled is false, will output fallback files from {projectOptions.AdditionalCompilationOutputDirectory}." );

                    ExecuteFromAdditionalCompilationOutputFiles( context, projectOptions );

                    return;
                }

                // Execute the pipeline.
                if ( !DesignTimeAspectPipelineFactory.Instance.TryExecute(
                    projectOptions,
                    compilation,
                    context.CancellationToken,
                    out var compilationResult ) )
                {
                    Logger.Instance?.Write( $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}'): the pipeline failed." );

                    return;
                }

                // Add introduced syntax trees.
                var sourcesCount = 0;

                foreach ( var introducedSyntaxTree in compilationResult.IntroducedSyntaxTrees )
                {
                    sourcesCount++;
                    context.AddSource( introducedSyntaxTree.Name, introducedSyntaxTree.GeneratedSyntaxTree.GetText() );
                }

                Logger.Instance?.Write( $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}'): {sourcesCount} sources generated." );

                // We don't report diagnostics because it seems to be without effect.
                // All diagnostics are reported by the analyzer.
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        private static void ExecuteFromAdditionalCompilationOutputFiles( GeneratorExecutionContext context, IProjectOptions projectOptions )
        {
            var serviceProvider = ServiceProvider.Empty.WithServices( projectOptions );
            var provider = new AdditionalCompilationOutputFileProvider( serviceProvider );

            if ( projectOptions.AdditionalCompilationOutputDirectory == null )
            {
                return;
            }

            var sourcesCount = 0;

            foreach ( var file in provider.GetAdditionalCompilationOutputFiles()
                .Where(
                    f => f.Kind == AdditionalCompilationOutputFileKind.DesignTimeGeneratedCode
                         && StringComparer.Ordinal.Equals( Path.GetExtension( f.Path ), ".cs" ) ) )
            {
                using var stream = file.GetStream();
                context.AddSource( Path.GetFileName( file.Path ), SourceText.From( stream ) );
                sourcesCount++;
            }

            Logger.Instance?.Write( $"DesignTimeSourceGenerator.Execute('{context.Compilation.AssemblyName}'): {sourcesCount} sources generated." );
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
            this._isEnabled = !MetalamaCompilerInfo.IsActive;
        }
    }
}