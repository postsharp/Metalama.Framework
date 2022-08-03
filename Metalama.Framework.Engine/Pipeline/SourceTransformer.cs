// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Compiler;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Metalama. An implementation of Metalama.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class SourceTransformer : ISourceTransformer
    {
        public void Execute( TransformerContext context )
        {
            var serviceProvider = ServiceProviderFactory.GetServiceProvider( nextServiceProvider: context.Services );

            var applicationInfoProvider = (IApplicationInfoProvider?) context.Services.GetService( typeof(IApplicationInfoProvider) );

            // Try.Metalama doesn't provide any backstage services.
            IApplicationInfo? previousApplication = null;

            if ( applicationInfoProvider != null )
            {
                previousApplication = applicationInfoProvider.CurrentApplication;
                applicationInfoProvider.CurrentApplication = new MetalamaApplicationInfo();
            }

            try
            {
                // Try.Metalama ships its own handler. Having the default ICompileTimeExceptionHandler added earlier
                // is not possible, because it needs access to IExceptionReporter service, which comes from the TransformerContext.
                if ( serviceProvider.GetService<ICompileTimeExceptionHandler>() == null )
                {
                    serviceProvider = serviceProvider.WithService( new CompileTimeExceptionHandler( serviceProvider ) );
                }

                // Try.Metalama ships its own project options using the async-local service provider.
                var projectOptions = serviceProvider.GetService<IProjectOptions>();

                if ( projectOptions == null )
                {
                    projectOptions = new MSBuildProjectOptions( context.AnalyzerConfigOptionsProvider.GlobalOptions, context.Plugins, context.Options );
                    serviceProvider = serviceProvider.WithService( projectOptions );
                }

                serviceProvider = serviceProvider.WithProjectScopedServices( context.Compilation );

                try
                {
                    using CompileTimeAspectPipeline pipeline = new( serviceProvider, false );

                    // ReSharper disable once AccessToDisposedClosure
                    var pipelineResult =
                        TaskHelper.RunAndWait(
                            () => pipeline.ExecuteAsync(
                                new DiagnosticAdderAdapter( context.ReportDiagnostic ),
                                context.Compilation,
                                context.Resources.ToImmutableArray(),
                                CancellationToken.None ) );

                    if ( pipelineResult != null )
                    {
                        context.AddResources( pipelineResult.AdditionalResources );
                        context.AddSyntaxTreeTransformations( pipelineResult.SyntaxTreeTransformations );
                    }

                    HandleAdditionalCompilationOutputFiles( projectOptions, pipelineResult );
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
            finally
            {
                if ( applicationInfoProvider != null )
                {
                    applicationInfoProvider.CurrentApplication = previousApplication!;
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
                    using var stream = File.OpenWrite( fullPath );
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