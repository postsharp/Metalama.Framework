// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// A <see cref="ServiceProvider"/> factory for design-time processes. Note that it should not be invoked directly from Visual Studio -- this
/// process has its own factory.
/// </summary>
public static class DesignTimeServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile ServiceProvider? _serviceProvider;

    public static ServiceProvider GetServiceProvider()
    {
        if ( _serviceProvider == null )
        {
            lock ( _initializeSync )
            {
                if ( _serviceProvider == null )
                {
                    Logger.Initialize();

                    _serviceProvider = ServiceProviderFactory.GetServiceProvider( nextServiceProvider: new LoggingServiceProvider() );

                    _serviceProvider = _serviceProvider
                        .WithService( new DesignTimeAspectPipelineFactory( _serviceProvider, new CompileTimeDomain() ) );

                    if ( ProcessUtilities.ProcessKind != ProcessKind.DevEnv )
                    {
                        _serviceProvider = _serviceProvider.WithServices(
                            new CodeRefactoringDiscoveryService( _serviceProvider ),
                            new CodeActionExecutionService( _serviceProvider ) );
                    }
                    else
                    {
                        // The service will be registered by VsDesignTimeServiceProviderFactory.
                    }
                }
            }
        }

        return _serviceProvider;
    }

    private class LoggingServiceProvider : IServiceProvider
    {
        public object? GetService( Type serviceType ) => serviceType == typeof(ILoggerFactory) ? DiagnosticsService.Instance : null;
    }
}