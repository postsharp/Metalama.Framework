// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.DesignTime.VersionNeutral;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Services;

namespace Metalama.Framework.DesignTime.Services;

/// <summary>
/// A <see cref="GlobalServiceProvider"/> factory for design-time processes. Note that it should not be invoked directly from Visual Studio -- this
/// process has its own factory.
/// </summary>
public abstract class DesignTimeServiceProviderFactory
{
    private static readonly object _initializeSync = new();
    private static volatile DesignTimeServiceProviderFactory? _sharedFactory;
    private static volatile ServiceProvider<IGlobalService>? _sharedServiceProvider;

    private readonly IDesignTimeEntryPointManager _designTimeEntryPointManager;

    protected DesignTimeServiceProviderFactory( IDesignTimeEntryPointManager? designTimeEntryPointManager )
    {
        this._designTimeEntryPointManager = designTimeEntryPointManager ?? DesignTimeEntryPointManager.Instance;
    }

    internal static ServiceProvider<IGlobalService> GetSharedServiceProvider()
    {
        if ( ProcessUtilities.ProcessKind == ProcessKind.DevEnv )
        {
            return GetSharedServiceProvider<DesignTimeUserProcessServiceProviderFactory>();
        }
        else
        {
            return GetSharedServiceProvider<DesignTimeAnalysisProcessServiceProviderFactory>();
        }
    }

    protected virtual ServiceProvider<IGlobalService> AddServices( ServiceProvider<IGlobalService> serviceProvider )
        => serviceProvider
            .WithServiceConditional<IProjectOptionsFactory>( _ => new MSBuildProjectOptionsFactory() )
            .WithServiceConditional<IUserDiagnosticRegistrationService>( sp => new UserDiagnosticRegistrationService( sp ) );

    protected virtual CompilerServiceProvider CreateCompilerServiceProvider() => new();

    public static ServiceProvider<IGlobalService> GetSharedServiceProvider<T>()
        where T : DesignTimeServiceProviderFactory, new()
    {
        if ( MetalamaCompilerInfo.IsActive )
        {
            throw new InvalidOperationException( "This method cannot be called from the Metalama Compiler process." );
        }

        if ( _sharedServiceProvider == null )
        {
            lock ( _initializeSync )
            {
                if ( _sharedServiceProvider == null )
                {
                    var factory = new T();

                    DesignTimeServices.Initialize();

                    if ( Logger.DesignTimeEntryPointManager.Trace != null )
                    {
                        DesignTimeEntryPointManager.Instance.SetLogger( Logger.DesignTimeEntryPointManager.Trace.Log );
                    }

                    var serviceProvider = ServiceProviderFactory.GetServiceProvider();

                    _sharedServiceProvider = factory.GetServiceProvider( serviceProvider );
                    _sharedFactory = factory;

                    DesignTimeServices.Start( _sharedServiceProvider );
                }
            }
        }

        if ( _sharedFactory is not T )
        {
            throw new AssertionFailedException( $"The method was already called with T being '{_sharedFactory?.GetType()}', but now it's '{typeof(T)}'." );
        }

        return _sharedServiceProvider;
    }

    internal ServiceProvider<IGlobalService> GetServiceProvider( ServiceProvider<IGlobalService> serviceProvider )
    {
        // Add the services that may be required by the CompilerServiceProvider.
        serviceProvider = serviceProvider.WithUntypedService( typeof(IRpcExceptionHandler), new RpcExceptionHandler() );

        // Create a CompilerServiceProvider.
        var compilerServiceProvider = this.CreateCompilerServiceProvider();
        var entryPointConsumer = this._designTimeEntryPointManager.GetConsumer( CurrentContractVersions.All );

        serviceProvider = serviceProvider.WithUntypedService( typeof(IDesignTimeEntryPointConsumer), entryPointConsumer );

        // Add other services.
        serviceProvider = this.AddServices( serviceProvider );

        // At this point, all services have been created, so we can initialize our CompilerServiceProvider and
        // register it. Once it is registered, consumers can start using the services immediately, so it is important
        // that all initializations are done before we register the provider to the entry point.
        compilerServiceProvider.Initialize( serviceProvider );
        this._designTimeEntryPointManager.RegisterServiceProvider( compilerServiceProvider );

        return serviceProvider;
    }

    private sealed class RpcExceptionHandler : IRpcExceptionHandler
    {
        public void OnException( Exception e, ILogger logger ) => DesignTimeExceptionHandler.ReportException( e, logger );
    }
}