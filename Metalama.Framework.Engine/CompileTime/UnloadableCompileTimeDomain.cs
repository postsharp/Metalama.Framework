// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
#endif

#if !NET5_0_OR_GREATER
#pragma warning disable CA1822 // Can be made static
#endif

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// An implementation of <see cref="CompileTimeDomain"/> base on <c>AssemblyLoadContext</c> and able to unload
    /// itself. When compiled with .NET Standard (instead of .NET 6.0), the class has no unloading effect.
    /// </summary>
    public sealed class UnloadableCompileTimeDomain : CompileTimeDomain
    {
#if NET5_0_OR_GREATER
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly List<WeakReference> _loadedAssemblies = new();
        private readonly TaskCompletionSource<bool> _unloadedTask = new();
        private volatile int _disposeStatus;

        public UnloadableCompileTimeDomain()
        {
            CollectibleExecutionContext.RegisterDisposeAction( this.WaitForDisposal );
            this._assemblyLoadContext = new AssemblyLoadContext( "Metalama_" + Guid.NewGuid(), true );
        }

        public event Action? Unloaded;

        public override Assembly LoadAssembly( string path )
        {
            // When using LoadFromAssemblyPath, the file is locked and the lock is not disposed when the AssemblyLoadContext is unloaded.
            // Therefore, we're loading from bytes.

            using var peStream = RetryHelper.Retry( () => File.OpenRead( path ) );
            var pdbPath = Path.ChangeExtension( path, ".pdb" );
            using var pdbStream = File.Exists( pdbPath ) ? RetryHelper.Retry( () => File.OpenRead( pdbPath ) ) : null;
            var assembly = this._assemblyLoadContext.LoadFromStream( peStream, pdbStream );

            this._loadedAssemblies.Add( new WeakReference( assembly ) );

            return assembly;
        }
#endif

        [ExcludeFromCodeCoverage]
        public Task UnloadAndWaitAsync()
        {
            // Must call the base Dispose method to clear the base cache.
            this.Dispose( true );

#if NET5_0_OR_GREATER
            return this._unloadedTask.Task;
#else
    return Task.CompletedTask;
#endif
        }

        [ExcludeFromCodeCoverage]
        public void WaitForDisposal() => this.WaitForDisposalAsync().Wait();

        [ExcludeFromCodeCoverage]
        public Task WaitForDisposalAsync()
        {
#if NET5_0_OR_GREATER
            if ( this._disposeStatus == 0 )
            {
                throw new InvalidOperationException( "The Dispose method has not been called." );
            }

            return this._unloadedTask.Task;
#else
         return Task.CompletedTask;
#endif
        }

#if NET5_0_OR_GREATER

        private void WaitForDisposalCore()
        {
            var waits = 0;

            while ( true )
            {
                List<WeakReference> aliveAssemblies;

                // While waiting for disposal, we need to prevent any other thread from taking a reference to the list of assemblies
                // loaded in the AppDomain, because such reference would prevent the assembly from being unloaded.
                lock ( AppDomainUtility.Sync )
                {
                    aliveAssemblies = this._loadedAssemblies.Where( r => r.IsAlive ).ToList();
                }

                if ( aliveAssemblies.Count == 0 )
                {
                    this._unloadedTask.SetResult( true );

                    this.Unloaded?.Invoke();

                    return;
                }

                waits++;
                Thread.Sleep( 10 );
                GC.Collect();
                GC.WaitForFullGCComplete();

                if ( waits > 10 )
                {
                    var assemblies = string.Join( ",", aliveAssemblies.SelectEnumerable( r => ((Assembly) r.Target!).GetName().Name ) );

                    /* IF YOU ARE HERE BECAUSE YOU ARE DEBUGGING A MEMORY LEAK
                     * 
                     * Here are a few pointers:
                     *  - You need to use WinDbg and sos.dll
                     *  - To know where sos.dll is and how to load it in WinDbg, type `dotnet sos install`.
                     *  - Follow instructions in https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
                     */

                    var exception = new InvalidOperationException(
                        "The domain could not be unloaded. There are probably dangling references. " +
                        "The following assemblies are still loaded: " + assemblies + "." );

                    this._unloadedTask.SetException( exception );
                }
            }
        }

#endif

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

#if NET5_0_OR_GREATER

            if ( Interlocked.CompareExchange( ref this._disposeStatus, 1, 0 ) == 0 )
            {
                this._assemblyLoadContext.Unload();
                Task.Run( this.WaitForDisposalCore );
            }

#endif

            // We cannot wait for complete disposal synchronously because the TestResult object, lower in the stack, typically contains
            // a reference to a build-time assembly. So, we need to put the test out of the stack before the domain
            // can be completely disposed.
        }
    }
}