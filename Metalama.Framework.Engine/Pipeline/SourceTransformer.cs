// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Metalama. An implementation of Metalama.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
    public sealed class SourceTransformer : ISourceTransformer
    {
        public void Execute( TransformerContext context )
        {
            var serviceProvider = ServiceProviderFactory.GetServiceProvider( context.Services );

            try
            {
                // Try.Metalama ships its own handler. Having the default ICompileTimeExceptionHandler added earlier
                // is not possible, because it needs access to IExceptionReporter service, which comes from the TransformerContext.
                if ( serviceProvider.GetService<ICompileTimeExceptionHandler>() == null )
                {
                    serviceProvider = serviceProvider.WithService( new CompileTimeExceptionHandler( serviceProvider ) );
                }

                // Try.Metalama ships its own project options using the async-local service provider.
                var projectOptions = (IProjectOptions?) serviceProvider.GetService( typeof(IProjectOptions) );

                projectOptions ??= MSBuildProjectOptionsFactory.Default.GetProjectOptions(
                    context.AnalyzerConfigOptionsProvider,
                    context.Plugins,
                    context.Options );

                var projectServiceProvider = serviceProvider.WithProjectScopedServices( projectOptions, context.Compilation );                

                using CompileTimeAspectPipeline pipeline = new( projectServiceProvider );

                var taskRunner = serviceProvider.GetRequiredService<ITaskRunner>();

                // ReSharper disable once AccessToDisposedClosure
                var pipelineResult =
                    taskRunner.RunSynchronously(
                        () => pipeline.ExecuteAsync(
                            new DiagnosticAdderAdapter( context.ReportDiagnostic ),
                            context.Compilation,
                            context.Resources,
                            TestableCancellationToken.None ) );

                if ( pipelineResult.IsSuccessful )
                {
                    context.AddResources( pipelineResult.Value.AdditionalResources );
                    context.AddSyntaxTreeTransformations( pipelineResult.Value.SyntaxTreeTransformations );
                    HandleAdditionalCompilationOutputFiles( projectOptions, pipelineResult.Value );
                }
            }
            catch ( Exception e )
            {
                var isHandled = false;

                serviceProvider
                    .GetService<ICompileTimeExceptionHandler>()
                    ?.ReportException( e, context.ReportDiagnostic, false, out isHandled );

                if ( !isHandled )
                {
                    throw;
                }
            }
        }

        private static void HandleAdditionalCompilationOutputFiles( IProjectOptions projectOptions, CompileTimeAspectPipelineResult? pipelineResult )
        {
            if ( pipelineResult == null || projectOptions.AdditionalCompilationOutputDirectory == null )
            {
                return;
            }

            try
            {
                var existingFiles = new HashSet<string>();

                if ( Directory.Exists( projectOptions.AdditionalCompilationOutputDirectory ) )
                {
                    foreach ( var existingAuxiliaryFile in Directory.GetFiles(
                                 projectOptions.AdditionalCompilationOutputDirectory,
                                 "*",
                                 SearchOption.AllDirectories ) )
                    {
                        existingFiles.Add( existingAuxiliaryFile );
                    }
                }

                var finalFiles = new HashSet<string>();

                foreach ( var file in pipelineResult.AdditionalCompilationOutputFiles )
                {
                    var fullPath = GetFileFullPath( file );
                    finalFiles.Add( fullPath );
                }

                foreach ( var deletedAuxiliaryFile in existingFiles.Except( finalFiles ) )
                {
                    File.Delete( deletedAuxiliaryFile );
                }

                foreach ( var file in pipelineResult.AdditionalCompilationOutputFiles )
                {
                    var fullPath = GetFileFullPath( file );
                    Directory.CreateDirectory( Path.GetDirectoryName( fullPath )! );
                    using var stream = File.Open( fullPath, FileMode.Create );
                    file.WriteToStream( stream );
                }

                string GetFileFullPath( AdditionalCompilationOutputFile file )
                    => Path.Combine( projectOptions.AdditionalCompilationOutputDirectory, file.Kind.ToString(), file.Path );
            }
            catch
            {
                // TODO: Warn.
            }
        }
    }
}