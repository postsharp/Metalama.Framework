// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Preview;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.AnalysisProcess;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Project;

namespace Metalama.Framework.DesignTime.VisualStudio;

/// <summary>
/// Factory of <see cref="ServiceProvider"/> for both user and analysis Visual Studio processes. 
/// </summary>
public static class VsServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile ServiceProvider<IService>? _serviceProvider;

    public static ServiceProvider<IService> GetServiceProvider()
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
                            _serviceProvider = DesignTimeServiceProviderFactory.GetServiceProvider( true );

                            var userProcessRegistrationService = UserProcessServiceHubEndpoint.GetInstance( _serviceProvider );

                            _serviceProvider = _serviceProvider.WithService( userProcessRegistrationService );

                            var compilerServiceProvider = new CompilerServiceProvider(  _serviceProvider, CurrentContractVersions.All );

                            if ( Logger.DesignTimeEntryPointManager.Trace != null )
                            {
                                DesignTimeEntryPointManager.Instance.SetLogger( Logger.DesignTimeEntryPointManager.Trace.Log );
                            }

                            DesignTimeEntryPointManager.Instance.RegisterServiceProvider( compilerServiceProvider );

                            break;

                        case ProcessKind.RoslynCodeAnalysisService:

                            // TODO: DesignTimePipelineFactory requires AnalysisProcessServiceHubEndpoint here below.
                            _serviceProvider = DesignTimeServiceProviderFactory.GetServiceProvider( false );

                            _serviceProvider =
                                _serviceProvider.WithService( new TransformationPreviewServiceImpl( _serviceProvider ) );

                            // AnalysisProcessEndpoint depends on the services added above.
                            if ( AnalysisProcessServiceHubEndpoint.TryStart(
                                    _serviceProvider,
                                    CancellationToken.None,
                                    out var endpointRegistrationApiProvider ) )
                            {
                                _serviceProvider = _serviceProvider.WithService( endpointRegistrationApiProvider );

                                var analysisProcessEndpoint = AnalysisProcessEndpoint.GetInstance( _serviceProvider );
                                _serviceProvider = _serviceProvider.WithService( analysisProcessEndpoint );
                            }

                            break;

                        default:
                            throw new AssertionFailedException( $"Unexpected process kind: {processKind}." );
                    }
                }
            }
        }

        return _serviceProvider;
    }
}