// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Utilities;
using PostSharp.Patterns;
using System;
using System.Collections.Generic;
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
    public class UnloadableCompileTimeDomain : CompileTimeDomain
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

        public void UnloadAndWait()
        {
            // Must call the base Dispose method to clear the base cache.
            this.Dispose( true );

            var waits = 0;

            while ( this._loadedAssemblies.Any( r => r.IsAlive ) )
            {
                waits++;
                Thread.Sleep( 10 );
                GC.Collect();
                GC.WaitForFullGCComplete();

                if ( waits > 10 )
                {
                    throw new AssertionFailedException( "The domain could not be unloaded. There are probably dangling references." );
                }
            }
        }

        public override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._assemblyLoadContext.Unload();
        }
    }
}