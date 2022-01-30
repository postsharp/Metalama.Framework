// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.DesignTime;

public static class DesignTimeServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile ServiceProvider? _serviceProvider;

    public static ServiceProvider GetServiceProvider( bool isImplementationProcess = true )
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

                    if ( isImplementationProcess )
                    {
                        _serviceProvider = _serviceProvider.WithService( new CodeActionDiscoveryServiceImpl( _serviceProvider ) );
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