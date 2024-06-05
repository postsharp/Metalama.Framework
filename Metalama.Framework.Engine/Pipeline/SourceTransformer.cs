// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Compiler;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using IExceptionReporter = Metalama.Backstage.Telemetry.IExceptionReporter;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// The main compile-time entry point of Metalama. An implementation of Metalama.Compiler's <see cref="ISourceTransformer"/>.
/// </summary>
[ExcludeFromCodeCoverage]
[UsedImplicitly]
public sealed partial class SourceTransformer : ISourceTransformerWithServices
{
    public IServiceProvider InitializeServices( InitializeServicesContext context )
    {
        if ( !BackstageServiceFactoryInitializer.IsInitialized )
        {
            var dotNetSdkDirectory = GetDotNetSdkDirectory( context.AnalyzerConfigOptionsProvider );

            var applicationInfo = new SourceTransformerApplicationInfo( context.Options.IsLongRunningProcess );

            var backstageOptions = new BackstageInitializationOptions( applicationInfo )
            {
                AddLicensing = true, AddUserInterface = true, AddSupportServices = true, DotNetSdkDirectory = dotNetSdkDirectory
            };

            BackstageServiceFactoryInitializer.Initialize( backstageOptions );
        }

        var backstageServiceProvider = BackstageServiceFactory.ServiceProvider;

        return new CompilerServiceProvider( backstageServiceProvider, context.AnalyzerConfigOptionsProvider );
    }

    private static string? GetDotNetSdkDirectory( AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider )
    {
        if ( !analyzerConfigOptionsProvider.GlobalOptions.TryGetValue( "build_property.NETCoreSdkBundledVersionsProps", out var propsFilePath )
             || string.IsNullOrEmpty( propsFilePath ) )
        {
            return null;
        }

        return Path.GetFullPath( Path.GetDirectoryName( propsFilePath )! );
    }

    private static void ReportException( Exception e, IServiceProvider serviceProvider, bool throwReporterExceptions )
    {
        try
        {
            var reporter = serviceProvider.GetBackstageService<IExceptionReporter>();
            reporter?.ReportException( e );
        }
        catch ( Exception reporterException )
        {
            if ( throwReporterExceptions )
            {
                throw new AggregateException( e, reporterException );
            }
        }
    }

    public void Execute( TransformerContext context )
    {
        var globalServices = ServiceProviderFactory.GetServiceProvider();

        try
        {
            // Try.Metalama ships its own handler. Having the default ICompileTimeExceptionHandler added earlier
            // is not possible, because it needs access to IExceptionReporter service, which comes from the TransformerContext.
            if ( globalServices.GetService<ICompileTimeExceptionHandler>() == null )
            {
                globalServices = globalServices.WithService( new CompileTimeExceptionHandler( globalServices ) );
            }

            // Try.Metalama ships its own project options using the async-local service provider.
            var projectOptions = (IProjectOptions?) globalServices.GetService( typeof(IProjectOptions) );

            projectOptions ??= MSBuildProjectOptionsFactory.Default.GetProjectOptions(
                context.AnalyzerConfigOptionsProvider,
                context.Options );
            
            var projectServiceProvider = globalServices
                .WithProjectScopedServices( projectOptions, context.Compilation )
                .WithService<IProjectLicenseConsumer>(
                    sp => ProjectLicenseConsumer.Create(
                        sp.GetRequiredBackstageService<ILicenseConsumptionService>(),
                        projectOptions.License,
                        projectOptions.IgnoreUserProfileLicense ? LicenseSourceKind.UserProfile : LicenseSourceKind.None,
                        context.ReportDiagnostic ) );

            using CompileTimeAspectPipeline pipeline = new( projectServiceProvider );

            var taskRunner = globalServices.GetRequiredService<ITaskRunner>();

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
                HandleSuppressions( context, projectServiceProvider, pipelineResult.Value.DiagnosticSuppressions );
            }
        }
        catch ( Exception e )
        {
            var isHandled = false;

            globalServices
                .GetService<ICompileTimeExceptionHandler>()
                ?.ReportException( e, context.ReportDiagnostic, false, out isHandled );

            if ( !isHandled )
            {
                throw;
            }
        }
        finally
        {
            globalServices.GetLoggerFactory().Flush();
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
            {
                return Path.Combine( projectOptions.AdditionalCompilationOutputDirectory, file.Kind.ToString(), file.Path );
            }
        }
        catch
        {
            // TODO: Warn.
        }
    }

    private static void HandleSuppressions(
        TransformerContext context,
        ProjectServiceProvider projectServiceProvider,
        ImmutableArray<ScopedSuppression> diagnosticSuppressions )
    {
        var userCodeInvoker = projectServiceProvider.GetRequiredService<UserCodeInvoker>();

        foreach ( var suppression in diagnosticSuppressions )
        {
            var declarationId = suppression.Declaration.GetSerializableId();

            UserCodeExecutionContext? executionContext = null;

            if ( suppression.Suppression.Filter != null )
            {
                executionContext = new UserCodeExecutionContext(
                    projectServiceProvider,
                    UserCodeDescription.Create( "evaluating suppression filter for {0} on {1}", suppression.Suppression.Definition, suppression.Declaration ) );
            }

            context.RegisterDiagnosticFilter(
                SuppressionFactories.CreateDescriptor( suppression.Suppression.Definition.SuppressedDiagnosticId ),
                request =>
                {
                    if ( suppression.Matches(
                            request.Diagnostic,
                            request.Compilation,
                            filter => userCodeInvoker.Invoke( filter, executionContext! ),
                            declarationId ) )
                    {
                        request.Suppress();
                    }
                } );
        }
    }
}