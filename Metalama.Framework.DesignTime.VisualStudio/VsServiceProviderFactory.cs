// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// Factory of <see cref="ServiceProvider"/> for both user and analysis Visual Studio processes. 
/// </summary>
public static class VsServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile ServiceProvider? _serviceProvider;

    public static ServiceProvider GetServiceProvider()
    {
        var processKind = ProcessUtilities.ProcessKind;

        if ( processKind == ProcessKind.Compiler )
        {
            throw new InvalidOperationException();
        }

        if ( _serviceProvider == null )
        {
            lock ( _initializeSync )
            {
                if ( _serviceProvider == null )
                {
                    switch ( processKind )
                    {
                        case ProcessKind.DevEnv:
                            _serviceProvider = DesignTimeServiceProviderFactory.GetServiceProvider();

                            var serviceClient = new UserProcessEndpoint( _serviceProvider );
                            _ = serviceClient.ConnectAsync();
                            _serviceProvider = _serviceProvider.WithService( serviceClient );

                            var compilerServiceProvider = new CompilerServiceProvider(
                                _serviceProvider,
                                ImmutableDictionary<string, int>.Empty.Add( "1.0", ContractsVersion.ContractVersion_1_0 ) );

                            if ( Logger.DesignTimeEntryPointManager.Trace != null )
                            {
                                DesignTimeEntryPointManager.Instance.SetLogger( Logger.DesignTimeEntryPointManager.Trace.Log );
                            }

                            DesignTimeEntryPointManager.Instance.RegisterServiceProvider( compilerServiceProvider );

                            break;

                        case ProcessKind.RoslynCodeAnalysisService:

                            _serviceProvider = DesignTimeServiceProviderFactory.GetServiceProvider();

                            _serviceProvider =
                                _serviceProvider.WithService( new TransformationPreviewServiceImpl( _serviceProvider ) );

                            // ServiceHost depends on the services added above.
                            var serviceHost = AnalysisProcessEndpoint.GetInstance( _serviceProvider );
                            _serviceProvider = _serviceProvider.WithService( serviceHost );

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