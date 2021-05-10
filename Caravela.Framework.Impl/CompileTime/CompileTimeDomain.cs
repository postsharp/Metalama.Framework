// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Tracks compile-time assemblies belonging to the same domain and implements CLR assembly resolution.
    /// The number of <see cref="CompileTimeDomain"/> in an <see cref="AppDomain"/>
    /// depends on the scenario: typically one per project at compile time, one per <see cref="AppDomain"/> at design time, and one per test
    /// at testing time.
    /// </summary>
    public class CompileTimeDomain : IDisposable
    {
        private static int _nextDomainId;
        private readonly ConcurrentDictionary<AssemblyIdentity, Assembly> _assemblyCache = new();
        private readonly int _domainId = Interlocked.Increment( ref _nextDomainId );

        public CompileTimeDomain()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.ResolveAssemblyReference;
        }

        /// <summary>
        /// Loads an assembly in the CLR. The default implementation is compatible with the .NET Framework,
        /// but it can be overwritten for .NET Core.
        /// </summary>
        protected virtual Assembly LoadAssembly( string path ) => Assembly.LoadFile( path );

        private Assembly? ResolveAssemblyReference( object sender, ResolveEventArgs args )
        {
            var assemblyIdentity = new AssemblyName( args.Name ).ToAssemblyIdentity();

            if ( this._assemblyCache.TryGetValue( assemblyIdentity, out var assembly ) )
            {
                return assembly;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an assembly given its <see cref="AssemblyIdentity"/> and image, or loads it.
        /// </summary>
        internal Assembly GetOrLoadAssembly( AssemblyIdentity compileTimeIdentity, string path )
            => this._assemblyCache.GetOrAdd( compileTimeIdentity, _ => this.LoadAssembly( path ) );

        public override string ToString() => this._domainId.ToString();

        public virtual void Dispose( bool disposing )
        {
            this._assemblyCache.Clear();

            // We should unload assemblies if we can, but this is a .NET Core feature only.
            AppDomain.CurrentDomain.AssemblyResolve -= this.ResolveAssemblyReference;
        }

        public void Dispose() => this.Dispose( true );
    }
}