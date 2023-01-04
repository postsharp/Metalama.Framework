// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// A <see cref="GlobalServiceProvider"/> factory for design-time processes. Note that it should not be invoked directly from Visual Studio -- this
/// process has its own factory.
/// </summary>
public static class DesignTimeServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile ServiceProvider<IGlobalService>? _serviceProvider;
    private static bool _isInitializedAsUserProcess;

    internal static ServiceProvider<IGlobalService> GetServiceProvider() => GetServiceProvider( ProcessUtilities.ProcessKind == ProcessKind.DevEnv );

    public static ServiceProvider<IGlobalService> GetServiceProvider(
        bool isUserProcess,
        Func<CompilerServiceProvider>? createCompilerServiceProvider = null )
    {
        if ( MetalamaCompilerInfo.IsActive )
        {
            throw new InvalidOperationException( "This method cannot be called from the Metalama Compiler process." );
        }

        if ( _serviceProvider == null )
        {
            lock ( _initializeSync )
            {
                if ( _serviceProvider == null )
                {
                    _isInitializedAsUserProcess = isUserProcess;

                    DesignTimeServices.Initialize();

                    if ( Logger.DesignTimeEntryPointManager.Trace != null )
                    {
                        DesignTimeEntryPointManager.Instance.SetLogger( Logger.DesignTimeEntryPointManager.Trace.Log );
                    }

                    _serviceProvider = ServiceProviderFactory.GetServiceProvider();
                    _serviceProvider = _serviceProvider.WithUntypedService( typeof(IRpcExceptionHandler), new RpcExceptionHandler() );

                    if ( !isUserProcess )
                    {
                        // Initialize the event hub.
                        _serviceProvider = _serviceProvider
                            .WithServices( new AnalysisProcessEventHub( _serviceProvider ) );

                        switch ( ProcessUtilities.ProcessKind )
                        {
                            case ProcessKind.Rider:
                                _serviceProvider = _serviceProvider.WithService( new LocalWorkspaceProvider( _serviceProvider ) );

                                break;

                            default:
                                if ( RemoteWorkspaceProvider.TryCreate( _serviceProvider, out var workspaceProvider ) )
                                {
                                    _serviceProvider = _serviceProvider.WithService( workspaceProvider );
                                }

                                break;
                        }

                        // Add the version-neutral entry point manager to the service collection.
                        createCompilerServiceProvider ??= () => new CompilerServiceProvider();
                        var compilerServiceProvider = createCompilerServiceProvider();

                        DesignTimeEntryPointManager.Instance.RegisterServiceProvider( compilerServiceProvider );
                        var entryPointConsumer = DesignTimeEntryPointManager.Instance.GetConsumer( CurrentContractVersions.All );

                        _serviceProvider = _serviceProvider.WithUntypedService( typeof(IDesignTimeEntryPointConsumer), entryPointConsumer );

                        // Add the pipeline factory.
                        var pipelineFactory = new DesignTimeAspectPipelineFactory( _serviceProvider, new CompileTimeDomain() );
                        _serviceProvider = _serviceProvider.WithServices( pipelineFactory );

                        // Initialize the CompilerServiceProvider.
                        compilerServiceProvider.Initialize( _serviceProvider );

                        // Add more services.
                        _serviceProvider = _serviceProvider.WithServices(
                            new CodeRefactoringDiscoveryService( _serviceProvider ),
                            new CodeActionExecutionService( _serviceProvider ),
                            new CodeLensServiceImpl( _serviceProvider ) );
                    }
                }
            }
        }

        if ( _isInitializedAsUserProcess != isUserProcess )
        {
            throw new AssertionFailedException( "The method was already called with a different value of the isUserProcess parameter." );
        }

        return _serviceProvider;
    }

    private sealed class RpcExceptionHandler : IRpcExceptionHandler
    {
        public void OnException( Exception e, ILogger logger ) => DesignTimeExceptionHandler.ReportException( e, logger );
    }
}