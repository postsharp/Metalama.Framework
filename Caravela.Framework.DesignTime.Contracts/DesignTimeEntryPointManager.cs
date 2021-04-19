// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Contracts
{
    /// <summary>
    /// Exposes a global connection point between compiler assemblies, included in NuGet packages and loaded by Roslyn,
    /// and the UI assemblies, included in the VSX and loaded by Visual Studio. Compiler assemblies register
    /// themselves using <see cref="IDesignTimeEntryPointManager.RegisterServiceProvider"/> and UI assemblies get the
    /// interface using <see cref="IDesignTimeEntryPointManager.GetServiceProviderAsync"/>.
    /// Since VS session can contain projects with several versions of Caravela, this class has the responsibility
    /// to match versions.
    /// </summary>
    public class DesignTimeEntryPointManager : IDesignTimeEntryPointManager
    {
        private const string _appDomainDataName = "Caravela.Framework.DesignTime.Contracts.DesignTimeEntryPointManager";

        private static readonly ConcurrentDictionary<Version, Task<ICompilerServiceProvider>> _getProviderTasks =
            new();

        public static Version MatchAllVersion { get; } = new( 9999, 99 );

        public static IDesignTimeEntryPointManager Instance { get; }

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

        private DesignTimeEntryPointManager() { }

        private readonly object _sync = new();
        private volatile TaskCompletionSource<ICompilerServiceProvider> _registrationTask = new();
        private ImmutableHashSet<ICompilerServiceProvider> _entryPoints = ImmutableHashSet<ICompilerServiceProvider>.Empty;

        async ValueTask<ICompilerServiceProvider?> IDesignTimeEntryPointManager.GetServiceProviderAsync( Version version, CancellationToken cancellationToken )
        {
            var task = _getProviderTasks.GetOrAdd( version, this.GetProviderForVersion );

            if ( !task.IsCompleted )
            {
                var taskCancelled = new TaskCompletionSource<bool>();

                using ( cancellationToken.Register( () => taskCancelled.SetCanceled() ) )
                {
                    await Task.WhenAny( task, taskCancelled.Task );
                }
            }

            return task.Result;
        }

        private async Task<ICompilerServiceProvider> GetProviderForVersion( Version version )
        {
            while ( true )
            {
                lock ( this._sync )
                {
                    foreach ( var entryPoint in this._entryPoints )
                    {
                        if ( version == MatchAllVersion || entryPoint.Version == version )
                        {
                            return entryPoint;
                        }
                    }
                }

                await this._registrationTask.Task;
            }
        }

        void IDesignTimeEntryPointManager.RegisterServiceProvider( ICompilerServiceProvider entryPoint )
        {
            lock ( this._sync )
            {
                entryPoint.Unloaded += this.OnUnloaded;
                this._entryPoints = this._entryPoints.Add( entryPoint );

                // The order here is important.
                var oldRegistrationTask = this._registrationTask;
                this._registrationTask = new TaskCompletionSource<ICompilerServiceProvider>();
                oldRegistrationTask.SetResult( entryPoint );
            }
        }

        Version IDesignTimeEntryPointManager.Version => this.GetType().Assembly.GetName().Version;

        private void OnUnloaded( ICompilerServiceProvider entryPoint )
        {
            lock ( this._sync )
            {
                this._entryPoints = this._entryPoints.Remove( entryPoint );
            }
        }
    }
}