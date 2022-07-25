// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if NET5_0_OR_GREATER
using Metalama.Backstage.Utilities;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
#endif

namespace Metalama.TestFramework
{
    /// <summary>
    /// An implementation of <see cref="CompileTimeDomain"/> base on <c>AssemblyLoadContext</c> and able to unload
    /// itself. When compiled with .NET Standard (instead of .NET 5.0), the class has no unloading effect.
    /// </summary>
    public sealed class UnloadableCompileTimeDomain : CompileTimeDomain
    {
#if NET5_0_OR_GREATER
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly List<WeakReference> _loadedAssemblies = new();

        public UnloadableCompileTimeDomain()
        {
            this._assemblyLoadContext = new AssemblyLoadContext( "Metalama_" + Guid.NewGuid(), true );
        }

        protected override Assembly LoadAssembly( string path )
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
        public void UnloadAndWait()
        {
            // Must call the base Dispose method to clear the base cache.
            this.Dispose( true );

            this.WaitForDisposal();
        }

#if !NET5_0_OR_GREATER
#pragma warning disable CA1822 // Can be made static
#endif
        [ExcludeFromCodeCoverage]
        public void WaitForDisposal()
        {
#if NET5_0_OR_GREATER
            var waits = 0;

            // While waiting for disposal, we need to prevent any other thread from taking a reference to the list of assemblies
            // loaded in the AppDomain, because such reference would prevent the assembly from being unloaded.
            lock ( AppDomainUtility.Sync )
            {
                while ( this._loadedAssemblies.Any( r => r.IsAlive ) )
                {
                    waits++;
                    Thread.Sleep( 10 );
                    GC.Collect();
                    GC.WaitForFullGCComplete();

                    if ( waits > 10 )
                    {
                        var assemblies = string.Join(
                            ",",
                            this._loadedAssemblies.Where( r => r.IsAlive ).Select( r => ((Assembly) r.Target!).GetName().Name ) );

                        /* IF YOU ARE HERE BECAUSE YOU ARE DEBUGGING A MEMORY LEAK
                         * 
                         * Here are a few pointers:
                         *  - You need to use WinDbg and sos.dll
                         *  - To know where sos.dll is and how to load it in WinDbg, type `dotnet sos install`.
                         *  - Follow instructions in https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
                         */
                        MiniDumper.Write();

                        throw new InvalidOperationException(
                            "The domain could not be unloaded. There are probably dangling references. " +
                            "The following assemblies are still loaded: " + assemblies + "." );
                    }
                }
            }
#endif
        }
#if !NET5_0_OR_GREATER
#pragma warning restore CA1822 // Can be made static
#endif

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

#if NET5_0_OR_GREATER
            this._assemblyLoadContext.Unload();
            TestExecutionContext.RegisterDisposeAction( this.WaitForDisposal );
#endif

            // We cannot wait for complete disposal here because the TestResult object, lower in the stack, typically contains
            // a reference to a build-time assembly. So, we need to put the test out of the stack before the domain
            // can be completely disposed.
        }
    }
}