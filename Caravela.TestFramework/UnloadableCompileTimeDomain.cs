// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Utilities;
using PostSharp.Patterns;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Caravela.TestFramework
{
    /// <summary>
    /// An implementation of <see cref="CompileTimeDomain"/> base on <see cref="AssemblyLoadContext"/> and able to unload
    /// itself. Not compatible with .NET Framework.
    /// </summary>
    internal class UnloadableCompileTimeDomain : CompileTimeDomain
    {
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly List<WeakReference> _loadedAssemblies = new();

        public UnloadableCompileTimeDomain()
        {
            this._assemblyLoadContext = new AssemblyLoadContext( "Caravela_" + Guid.NewGuid(), true );
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

        [ExcludeFromCodeCoverage]
        public void UnloadAndWait()
        {
            // Must call the base Dispose method to clear the base cache.
            this.Dispose( true );

            this.WaitForDisposal();
        }

        [ExcludeFromCodeCoverage]
        public void WaitForDisposal()
        {
            var waits = 0;

            while ( this._loadedAssemblies.Any( r => r.IsAlive ) )
            {
                waits++;
                Thread.Sleep( 10 );
                GC.Collect();
                GC.WaitForFullGCComplete();

                if ( waits > 10 )
                {
                    var assemblies = string.Join( ",", this._loadedAssemblies.Where( r => r.IsAlive ).Select( r => ((Assembly) r.Target!).GetName().Name ) );

                    throw new AssertionFailedException(
                        "The domain could not be unloaded. There are probably dangling references. " +
                        "The following assemblies are still loaded: " + assemblies + "." );
                }
            }
        }

        public override void Dispose( bool disposing )
        {
            base.Dispose( disposing );

            this._assemblyLoadContext.Unload();
            TestExecutionContext.RegisterDisposedDomain( this );

            // We cannot wait for complete disposal here because the TestResult object, lower in the stack, typically contains
            // a reference to a build-time assembly. So, we need to put the test out of the stack before the domain
            // can be completely disposed.
        }
    }
}