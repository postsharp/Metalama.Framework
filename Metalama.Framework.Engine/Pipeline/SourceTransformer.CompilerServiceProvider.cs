﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Compiler.Services;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using IExceptionReporter = Metalama.Backstage.Telemetry.IExceptionReporter;
using ILogger = Metalama.Compiler.Services.ILogger;

namespace Metalama.Framework.Engine.Pipeline;

public sealed partial class SourceTransformer
{
    private sealed class CompilerServiceProvider : IDisposableServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, object> _services = new();
        private readonly IDisposable _scope;
        private readonly IUsageSession? _session;

        public CompilerServiceProvider( IServiceProvider serviceProvider, AnalyzerConfigOptionsProvider contextAnalyzerConfigOptionsProvider )
        {
            this._serviceProvider = serviceProvider;

            var options = new MSBuildProjectOptions( contextAnalyzerConfigOptionsProvider.GlobalOptions );

            var loggerFactory = serviceProvider.GetLoggerFactory();
            this._scope = loggerFactory.EnterScope( options.AssemblyName ?? "Unnamed" );

            this._services.Add( typeof(ILoggerFactory), loggerFactory );
            this._services.Add( typeof(ILogger), new LoggerAdapter( loggerFactory.GetLogger( "Compiler" ) ) );
            this._services.Add( typeof(IExceptionReporter), new ExceptionReporterAdapter( serviceProvider.GetBackstageService<IExceptionReporter>() ) );
            
            // Initialize usage reporting.
            try
            {
                if ( serviceProvider.GetBackstageService<IUsageReporter>() is { } usageReporter && options.AssemblyName != null
                                                                                                && usageReporter.ShouldReportSession( options.AssemblyName ) )
                {
                    this._session = usageReporter.StartSession( "TransformerUsage" );
                }
            }
            catch ( Exception e )
            {
                ReportException( e, serviceProvider, false );

                // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
            }
        }

        public object? GetService( Type serviceType )
        {
            this._services.TryGetValue( serviceType, out var service );

            return service;
        }

        public void DisposeServices( Action<Diagnostic> reportDiagnostic )
        {
            // Report usage.
            try
            {
                this._session?.Dispose();
            }
            catch ( Exception e )
            {
                ReportException( e, this._serviceProvider, false );

                // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
            }

            // Close the scope.
            this._scope.Dispose();
        }
    }
}