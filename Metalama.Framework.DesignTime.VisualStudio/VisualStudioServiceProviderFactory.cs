// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;

namespace Metalama.Framework.DesignTime.VisualStudio;

public static class VisualStudioServiceProviderFactory
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
                    _serviceProvider = DesignTimeServiceProviderFactory.GetServiceProvider();

                    switch ( DebuggingHelper.ProcessKind )
                    {
                        case ProcessKind.DevEnv:
                            var serviceClient = new ServiceClient();
                            _ = serviceClient.ConnectAsync();
                            _serviceProvider = _serviceProvider.WithService( serviceClient );

                            break;

                        case ProcessKind.RoslynCodeAnalysisService:
                            var serviceHost = ServiceHost.GetInstance( _serviceProvider );

                            if ( serviceHost != null )
                            {
                                _serviceProvider = _serviceProvider.WithService( serviceHost );
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }
            }
        }

        return _serviceProvider;
    }
}