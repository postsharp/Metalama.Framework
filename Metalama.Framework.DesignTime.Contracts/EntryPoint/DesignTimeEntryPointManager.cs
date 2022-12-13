// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts.EntryPoint
{
    /// <summary>
    /// Exposes a global connection point between compiler assemblies, included in NuGet packages and loaded by Roslyn,
    /// and the UI assemblies, included in the VSX and loaded by Visual Studio. Compiler assemblies register
    /// themselves using <see cref="IDesignTimeEntryPointManager.RegisterServiceProvider"/> and UI assemblies get the
    /// interface using <see cref="IDesignTimeEntryPointManager.GetConsumer"/> and then calling the methods of this interface.
    /// Since VS session can contain projects with several versions of Metalama, this class has the responsibility
    /// to match versions.
    /// </summary>
    public sealed partial class DesignTimeEntryPointManager : IDesignTimeEntryPointManager
    {
        private const string _appDomainDataName = "Metalama.Framework.DesignTime.Contracts.DesignTimeEntryPointManager";

        [Obsolete( "Specify the exact version." )]
        public static Version MatchAllVersion { get; } = new( 9999, 99 );

        [ExcludeFromCodeCoverage]
        public static IDesignTimeEntryPointManager Instance { get; }

        [ExcludeFromCodeCoverage]
        static DesignTimeEntryPointManager()
        {
            // Note that there maybe many instances of this class in the AppDomain, so it needs to make sure it uses a shared point of contact.
            // We're using a named AppDomain data slot for this. We have to synchronize access using a named semaphore.

            using var semaphore = new Semaphore( 1, 1, _appDomainDataName );

            try
            {
                try
                {
                    semaphore.WaitOne();
                }
                catch ( AbandonedMutexException ) { }

                var untypedSharedInstance = AppDomain.CurrentDomain.GetData( _appDomainDataName );
                var sharedInstance = (IDesignTimeEntryPointManager?) untypedSharedInstance;

                if ( sharedInstance != null )
                {
                    Instance = sharedInstance;
                }
                else
                {
                    Instance = new DesignTimeEntryPointManager();
                    AppDomain.CurrentDomain.SetData( _appDomainDataName, Instance );
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        // The constructor is public because it is used for tests, so we don't base tests on the singleton instance.
        // ReSharper disable once EmptyConstructor
        public DesignTimeEntryPointManager() { }

        private readonly object _sync = new();
        private volatile TaskCompletionSource<ICompilerServiceProvider> _registrationTask = new();
        private volatile ImmutableHashSet<ICompilerServiceProvider> _providers = ImmutableHashSet<ICompilerServiceProvider>.Empty;
        private int _nextObserverId;

        private volatile ImmutableDictionary<int, ServiceProviderEventHandler> _observers =
            ImmutableDictionary<int, ServiceProviderEventHandler>.Empty;

        private LogAction? _logger;

        public void SetLogger( LogAction? logger ) => this._logger = logger;

        public IDesignTimeEntryPointConsumer GetConsumer( ContractVersion[] contractVersions )
            => new Consumer( this, contractVersions.ToImmutableDictionary( i => i.Version, i => i.Revision ) );

        void IDesignTimeEntryPointManager.RegisterServiceProvider( ICompilerServiceProvider provider )
        {
            lock ( this._sync )
            {
                this._providers = this._providers.Add( provider );

                this._logger?.Invoke( $"Registering service provider v{provider.Version}." );

                // The order here is important.
                var oldRegistrationTask = this._registrationTask;
                this._registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
                oldRegistrationTask.SetResult( provider );

                // Send notifications.
                foreach ( var observer in this._observers )
                {
                    this._logger?.Invoke( $"Notifying observer." );

                    observer.Value.Invoke( provider );
                }
            }
        }

        Version IDesignTimeEntryPointManager.Version => this.GetType().Assembly.GetName().Version!;
    }
}