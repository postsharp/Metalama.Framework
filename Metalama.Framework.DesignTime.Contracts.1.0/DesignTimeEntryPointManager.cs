// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Exposes a global connection point between compiler assemblies, included in NuGet packages and loaded by Roslyn,
    /// and the UI assemblies, included in the VSX and loaded by Visual Studio. Compiler assemblies register
    /// themselves using <see cref="IDesignTimeEntryPointManager.RegisterServiceProvider"/> and UI assemblies get the
    /// interface using <see cref="IDesignTimeEntryPointManager.GetConsumer"/> and then calling the methods of this interface.
    /// Since VS session can contain projects with several versions of Metalama, this class has the responsibility
    /// to match versions.
    /// </summary>
    public class DesignTimeEntryPointManager : IDesignTimeEntryPointManager
    {
        private const string _appDomainDataName = "Metalama.Framework.DesignTime.Contracts.DesignTimeEntryPointManager";

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
                semaphore.WaitOne();
                var oldInstance = (IDesignTimeEntryPointManager?) AppDomain.CurrentDomain.GetData( _appDomainDataName );

                if ( oldInstance != null )
                {
                    Instance = oldInstance;
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

        private volatile ImmutableDictionary<int, IObserver<ICompilerServiceProvider>> _observers =
            ImmutableDictionary<int, IObserver<ICompilerServiceProvider>>.Empty;

        public IDesignTimeEntryPointConsumer GetConsumer( ImmutableDictionary<string, int> contractVersions )
            => new DesignTimeEntryPointConsumer( this, contractVersions );

        void IDesignTimeEntryPointManager.RegisterServiceProvider( ICompilerServiceProvider provider )
        {
            lock ( this._sync )
            {
                this._providers = this._providers.Add( provider );

                // The order here is important.
                var oldRegistrationTask = this._registrationTask;
                this._registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
                oldRegistrationTask.SetResult( provider );

                // Send notifications.
                foreach ( var observer in this._observers )
                {
                    observer.Value.OnNext( provider );
                }
            }
        }

        Version IDesignTimeEntryPointManager.Version => this.GetType().Assembly.GetName().Version!;

        private class DesignTimeEntryPointConsumer : IDesignTimeEntryPointConsumer
        {
            private readonly DesignTimeEntryPointManager _parent;
            private readonly ImmutableDictionary<string, int> _contractVersions;
            private readonly ConcurrentDictionary<Version, Task<ICompilerServiceProvider>> _getProviderTasks = new();

            public DesignTimeEntryPointConsumer( DesignTimeEntryPointManager parent, ImmutableDictionary<string, int> contractVersions )
            {
                this._parent = parent;
                this._contractVersions = contractVersions;
            }

            private bool ValidateContractVersions( ImmutableDictionary<string, int> candidate )
            {
                foreach ( var supportedVersion in this._contractVersions )
                {
                    if ( candidate.TryGetValue( supportedVersion.Key, out var candidateVersion ) && candidateVersion != supportedVersion.Value )
                    {
                        return false;
                    }
                }

                return true;
            }

            public async ValueTask<ICompilerServiceProvider?> GetServiceProviderAsync( Version version, CancellationToken cancellationToken )
            {
                var task = this._getProviderTasks.GetOrAdd( version, this.GetProviderForVersionAsync );

                if ( !task.IsCompleted )
                {
                    var taskCancelled = new TaskCompletionSource<bool>();

#if NET5_0_OR_GREATER
                    await using ( cancellationToken.Register( () => taskCancelled.SetCanceled( cancellationToken ) ) )
#else
                    using ( cancellationToken.Register( () => taskCancelled.SetCanceled() ) )
#endif
                    {
                        await Task.WhenAny( task, taskCancelled.Task );

                        if ( taskCancelled.Task.IsCanceled )
                        {
                            throw new TaskCanceledException();
                        }
                    }
                }

                return task.Result;
            }

            public IEnumerable<ICompilerServiceProvider> GetRegisteredProviders() => this._parent._providers;

            public event Action<ICompilerServiceProvider>? ContractVersionMismatchDetected;

            private async Task<ICompilerServiceProvider> GetProviderForVersionAsync( Version version )
            {
                while ( true )
                {
                    lock ( this._parent._sync )
                    {
                        foreach ( var entryPoint in this._parent._providers )
                        {
                            if ( version == MatchAllVersion || entryPoint.Version == version )
                            {
                                if ( this.ValidateContractVersions( entryPoint.ContractVersions ) )
                                {
                                    return entryPoint;
                                }
                                else
                                {
                                    this.ContractVersionMismatchDetected?.Invoke( entryPoint );

                                    return new InvalidCompilerServiceProvider( entryPoint.Version, entryPoint.ContractVersions );
                                }
                            }
                        }
                    }

                    await this._parent._registrationTask.Task;
                }
            }

            /// <summary>
            /// Subscribes an observer, which will be invoked when a new <see cref="ICompilerServiceProvider"/> is registered.
            /// </summary>
            public IDisposable Subscribe( IObserver<ICompilerServiceProvider> observer )
            {
                lock ( this._parent._sync )
                {
                    foreach ( var provider in this._parent._providers )
                    {
                        observer.OnNext( provider );
                    }

                    var observerId = this._parent._nextObserverId++;
                    this._parent._observers = this._parent._observers.Add( observerId, observer );

                    return new ObserverCookie( this._parent, observerId );
                }
            }

            private class ObserverCookie : IDisposable
            {
                private readonly DesignTimeEntryPointManager _parent;
                private readonly int _id;

                public ObserverCookie( DesignTimeEntryPointManager parent, int id )
                {
                    this._parent = parent;
                    this._id = id;
                }

                public void Dispose()
                {
                    lock ( this._parent._sync )
                    {
                        this._parent._observers = this._parent._observers.Remove( this._id );
                    }
                }
            }
        }
    }
}