// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Preview;
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
                            var serviceClient = new ServiceClient( _serviceProvider );
                            _ = serviceClient.ConnectAsync();
                            _serviceProvider = _serviceProvider.WithService( serviceClient );

                            var compilerServiceProvider = new CompilerServiceProvider( _serviceProvider );
                            DesignTimeEntryPointManager.Instance.RegisterServiceProvider( compilerServiceProvider );

                            break;

                        case ProcessKind.RoslynCodeAnalysisService:
                            
                            _serviceProvider =
                                _serviceProvider.WithService( new TransformationPreviewServiceImpl( _serviceProvider ) );

                            // ServiceHost depends on the services added above.
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