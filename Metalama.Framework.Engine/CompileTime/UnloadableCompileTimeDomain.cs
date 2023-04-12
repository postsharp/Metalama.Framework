// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET5_0_OR_GREATER
using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="CompileTimeDomain"/> base on <c>AssemblyLoadContext</c> and able to unload
    /// itself. When compiled with .NET Standard (instead of .NET 6.0), the class has no unloading effect.
    /// </summary>
    public sealed class UnloadableCompileTimeDomain : CompileTimeDomain
    {
        private readonly List<WeakReference> _loadedAssemblies = new();
        private readonly TaskCompletionSource<bool> _unloadedTask = new();
        private readonly ITaskRunner _taskRunner;
        private volatile int _disposeStatus;
        private AssemblyLoadContext? _assemblyLoadContext;
        private int _isWaitingForDisposal;

        public UnloadableCompileTimeDomain( GlobalServiceProvider serviceProvider ) : base( serviceProvider )
        {
            CollectibleExecutionContext.RegisterDisposeAction( this.WaitForDisposal );
            this._assemblyLoadContext = new AssemblyLoadContext( "Metalama_" + Guid.NewGuid(), true );

            this._taskRunner = serviceProvider.GetRequiredService<ITaskRunner>();
        }

        public event Action? Unloaded;

        public event Action? UnloadTimeout;

        public override Assembly LoadAssembly( string path )
        {
            // When using LoadFromAssemblyPath, the file is locked and the lock is not disposed when the AssemblyLoadContext is unloaded.
            // Therefore, we're loading from bytes.

            using var peStream = RetryHelper.Retry( () => File.OpenRead( path ) );
            var pdbPath = Path.ChangeExtension( path, ".pdb" );
            using var pdbStream = File.Exists( pdbPath ) ? RetryHelper.Retry( () => File.OpenRead( pdbPath ) ) : null;
            var assembly = this._assemblyLoadContext.AssertNotNull().LoadFromStream( peStream, pdbStream );

            lock ( this._loadedAssemblies )
            {
                this._loadedAssemblies.Add( new WeakReference( assembly ) );
            }

            return assembly;
        }

        [ExcludeFromCodeCoverage]
        public Task UnloadAndWaitAsync()
        {
            // Must call the base Dispose method to clear the base cache.
            this.Dispose( true );

            return this._unloadedTask.Task;
        }

        [ExcludeFromCodeCoverage]
        private void WaitForDisposal()
        {
            this._taskRunner.RunSynchronously( this.WaitForDisposalAsync );
        }

        [ExcludeFromCodeCoverage]
        private Task WaitForDisposalAsync()
        {
            if ( this._disposeStatus == 0 )
            {
                throw new InvalidOperationException( "The Dispose method has not been called." );
            }

            return this.WaitForDisposalCoreAsync();
        }

        private async Task WaitForDisposalCoreAsync()
        {
            if ( Interlocked.CompareExchange( ref this._isWaitingForDisposal, 1, 0 ) != 0 )
            {
                // Another thread has won.
                await this._unloadedTask.Task;

                return;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                while ( true )
                {
                    List<WeakReference> aliveAssemblies;

                    // While waiting for disposal, we need to prevent any other thread from taking a reference to the list of assemblies
                    // loaded in the AppDomain, because such reference would prevent the assembly from being unloaded.
                    lock ( AppDomainUtility.Sync )
                    {
                        lock ( this._loadedAssemblies )
                        {
                            aliveAssemblies = this._loadedAssemblies.Where( r => r.IsAlive ).ToList();
                        }
                    }

                    if ( aliveAssemblies.Count == 0 )
                    {
                        this._unloadedTask.SetResult( true );

                        this.Unloaded?.Invoke();
                        this.Observer?.OnDomainUnloaded( this );

                        return;
                    }

                    await Task.Delay( 100 );

                    GC.Collect();
                    GC.WaitForFullGCComplete();

                    if ( stopwatch.Elapsed.TotalSeconds > 30 )
                    {
                        var assemblies = string.Join( ",", aliveAssemblies.SelectAsEnumerable( r => ((Assembly) r.Target!).GetName().Name ) );

                        // ReSharper disable CommentTypo

                        /* IF YOU ARE HERE BECAUSE YOU ARE DEBUGGING A MEMORY LEAK
                         * 
                         * Here are a few pointers:
                         *  - You need to use WinDbg and sos.dll.
                         *  - To install sos.dll, do `dotnet tool install --global dotnet-sos`.
                         *  - To know where sos.dll is and how to load it in WinDbg, type `dotnet sos install`.
                         *  - Follow instructions in https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability:
                         *      - !dumpheap -type LoaderAllocator
                         *      - !gcroot -all xxxxx
                         */

                        // ReSharper restore CommentTypo

                        this.UnloadTimeout?.Invoke();

                        throw new InvalidOperationException(
                            "The domain could not be unloaded. There are probably dangling references. " +
                            "The following assemblies are still loaded: " + assemblies + "." );
                    }
                }
            }
            catch ( Exception e )
            {
                this._unloadedTask.TrySetException( e );

                throw;
            }
        }

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

            if ( Interlocked.CompareExchange( ref this._disposeStatus, 1, 0 ) == 0 )
            {
                this._assemblyLoadContext?.Unload();
                this._assemblyLoadContext = null;
                _ = Task.Run( this.WaitForDisposalCoreAsync );
            }

            // We cannot wait for complete disposal synchronously because the TestResult object, lower in the stack, typically contains
            // a reference to a build-time assembly. So, we need to put the test out of the stack before the domain
            // can be completely disposed.
        }
    }
}
#endif