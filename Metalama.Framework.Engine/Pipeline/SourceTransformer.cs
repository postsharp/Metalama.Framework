// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Telemetry;
using Metalama.Compiler;
using Metalama.Compiler.Services;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using IExceptionReporter = Metalama.Backstage.Telemetry.IExceptionReporter;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Metalama. An implementation of Metalama.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
    public sealed class SourceTransformer : ISourceTransformerWithServices
    {
        public IServiceProvider? InitializeServices( InitializeServicesContext context )
        {
            var dotNetSdkDirectory = GetDotNetSdkDirectory( context.AnalyzerConfigOptionsProvider );

            var licenseOptions = GetLicensingOptions( context.AnalyzerConfigOptionsProvider );

            var applicationInfo = new SourceTransformerApplicationInfo(
                context.Options.IsLongRunningProcess,
                licenseOptions.IgnoreUnattendedProcessLicense );

            var backstageOptions = new BackstageInitializationOptions( applicationInfo, context.Compilation.AssemblyName )
            {
                OpenWelcomePage = true,
                AddLicensing = true,
                AddSupportServices = true,
                LicensingOptions = licenseOptions,
                DotNetSdkDirectory = dotNetSdkDirectory
            };
           
            BackstageServiceFactoryInitializer.Initialize( backstageOptions );
            
            var serviceProvider = BackstageServiceFactory.ServiceProvider;

            // Enforce licensing.
            if ( serviceProvider.GetBackstageService<ILicenseConsumptionService>() is { } licenseManager )
            {
                var projectPath = MSBuildProjectOptionsFactory.Default.GetProjectOptions( context.AnalyzerConfigOptionsProvider ).ProjectPath;
                var projectName = Path.GetFileNameWithoutExtension( projectPath ) ?? "unknown";

                if ( !licenseManager.CanConsume( LicenseRequirement.Free, projectName ) )
                {
                    // We only emit the generic invalid license error when no specific error message
                    // comes from the license manager to avoid confusion of users.
                    if ( !licenseManager.Messages.Any( m => m.IsError ) )
                    {
                        context.ReportDiagnostic( LicensingDiagnosticDescriptors.InvalidLicenseOverall.CreateRoslynDiagnostic( null, default ) );
                    }
                    else
                    {
                        // Errors are emitted by the MetalamaFrameworkServicesHolder.DisposeServices() method.
                    }

                    return null;
                }
            }

            return new CompilerServiceProvider( serviceProvider, context.Compilation.AssemblyName );
        }

        private sealed class CompilerServiceProvider : IDisposableServiceProvider
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly LoggerAdapter _logger;
            private readonly ExceptionReporterAdapter _exceptionReporter;
            private readonly IUsageSession? _usageReportingSession;

            public CompilerServiceProvider( IServiceProvider serviceProvider, string? projectName )
            {
                this._serviceProvider = serviceProvider;
                this._logger = new LoggerAdapter( serviceProvider.GetLoggerFactory().GetLogger( "Compiler" ) );
                this._exceptionReporter = new ExceptionReporterAdapter( serviceProvider.GetBackstageService<IExceptionReporter>() );
                
                // Initialize usage reporting.
                try
                {
                    if ( serviceProvider.GetBackstageService<IUsageReporter>() is { } usageReporter && projectName != null
                                                                                                    && usageReporter.ShouldReportSession( projectName ) )
                    {
                        this._usageReportingSession = usageReporter.StartSession( "TransformerUsage" );
                    }
                }
                catch ( Exception e )
                {
                    ReportException( e, serviceProvider, false );

                    // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
                }
            }

            public object? GetService( Type serviceType )
                => serviceType == typeof(Metalama.Compiler.Services.ILogger) ? this._logger
                    : serviceType == typeof(Metalama.Compiler.Services.IExceptionReporter) ? this._exceptionReporter
                    : null;

            public void DisposeServices( Action<Diagnostic> reportDiagnostic )
            {
                // Write all licensing messages that may have been emitted during the compilation.
                if ( this._serviceProvider.GetBackstageService<ILicenseConsumptionService>() is { } licenseManager )
                {
                    foreach ( var licensingMessage in licenseManager.Messages )
                    {
                        var diagnosticDefinition = licensingMessage.IsError
                            ? LicensingDiagnosticDescriptors.LicensingError
                            : LicensingDiagnosticDescriptors.LicensingWarning;

                        reportDiagnostic( diagnosticDefinition.CreateRoslynDiagnostic( null, licensingMessage.Text ) );
                    }
                }

                // Report usage.
                try
                {
                    this._usageReportingSession?.Dispose();
                }
                catch ( Exception e )
                {
                    ReportException( e, this._serviceProvider, false );

                    // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
                }

                // Close logs.
                // Logging has to be disposed as the last one, so it could be used until now.
                this._serviceProvider.GetLoggerFactory().Dispose();
            }
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

        private static LicensingInitializationOptions GetLicensingOptions( AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider )
        {
            // Load license keys from build options.
            string? projectLicense = null;

            if ( analyzerConfigOptionsProvider.GlobalOptions.TryGetValue( "build_property.MetalamaLicense", out var licenseProperty ) )
            {
                projectLicense = licenseProperty.Trim();
            }

            if ( !(analyzerConfigOptionsProvider.GlobalOptions.TryGetValue( "build_property.MetalamaIgnoreUserLicenses", out var ignoreUserLicensesProperty )
                   && bool.TryParse( ignoreUserLicensesProperty, out var ignoreUserLicenses )) )
            {
                ignoreUserLicenses = false;
            }

            return new LicensingInitializationOptions
            {
                ProjectLicense = projectLicense,
                IgnoreUserProfileLicenses = ignoreUserLicenses,
                IgnoreUnattendedProcessLicense = ignoreUserLicenses
            };
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
            var serviceProvider = ServiceProviderFactory.GetServiceProvider();

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