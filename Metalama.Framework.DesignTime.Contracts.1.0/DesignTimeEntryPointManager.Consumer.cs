// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.DesignTime.Contracts;

public partial class DesignTimeEntryPointManager
{
    private class Consumer : IDesignTimeEntryPointConsumer
    {
        private readonly DesignTimeEntryPointManager _parent;
        private readonly ImmutableDictionary<string, int> _contractVersions;
        private readonly ConcurrentDictionary<Version, Task<ICompilerServiceProvider>> _getProviderTasks = new();

        public Consumer( DesignTimeEntryPointManager parent, ImmutableDictionary<string, int> contractVersions )
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
            this._parent._logger?.Invoke( $"GetProviderForVersionAsync({version})" );

            while ( true )
            {
                lock ( this._parent._sync )
                {
                    foreach ( var entryPoint in this._parent._providers )
                    {
#pragma warning disable CS0618
                        if ( version == MatchAllVersion || entryPoint.Version == version )
#pragma warning restore CS0618
                        {
                            if ( this.ValidateContractVersions( entryPoint.ContractVersions ) )
                            {
                                this._parent._logger?.Invoke(
                                    $"GetProviderForVersionAsync({version}): found a valid version with matching contract versions." );

                                return entryPoint;
                            }
                            else
                            {
                                this._parent._logger?.Invoke(
                                    $"GetProviderForVersionAsync({version}): found a valid version, but contract versions do not match." );

                                this.ContractVersionMismatchDetected?.Invoke( entryPoint );

                                return new InvalidCompilerServiceProvider( entryPoint.Version, entryPoint.ContractVersions );
                            }
                        }
                    }
                }

                this._parent._logger?.Invoke( $"GetProviderForVersionAsync({version}): waiting for a new registration." );

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