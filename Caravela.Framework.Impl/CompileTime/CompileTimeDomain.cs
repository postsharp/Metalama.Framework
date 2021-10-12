// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
#if NETFRAMEWORK
using System.Linq;
#endif

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

        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies = new();

        public CompileTimeDomain()
        {
            if ( RuntimeInformation.FrameworkDescription.StartsWith( ".NET Framework", StringComparison.Ordinal ) )
            {
                AppDomain.CurrentDomain.AssemblyResolve += this.OnAssemblyResolve;
            }
        }

        private Assembly? OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            var assemblyName = new AssemblyName( args.Name );

            if ( this._loadedAssemblies.TryGetValue( assemblyName.Name, out var candidateAssembly )
                 && AssemblyName.ReferenceMatchesDefinition( assemblyName, candidateAssembly.GetName() ) )
            {
                return candidateAssembly;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads an assembly in the CLR. The default implementation is compatible with the .NET Framework,
        /// but it can be overwritten for .NET Core.
        /// </summary>
        protected virtual Assembly LoadAssembly( string path ) => Assembly.LoadFile( path );

        /// <summary>
        /// Gets an assembly given its <see cref="AssemblyIdentity"/> and image, or loads it.
        /// </summary>
        internal Assembly GetOrLoadAssembly( AssemblyIdentity compileTimeIdentity, string path )
        {
            var assembly = this._assemblyCache.GetOrAdd( compileTimeIdentity, _ => this.LoadAssembly( path ) );

            // CompileTimeDomain is used only for compile-time assemblies, which always have a unique name, so we can have safely
            // index assemblies by name only.
            if ( !this._loadedAssemblies.TryAdd( compileTimeIdentity.Name, assembly ) )
            {
                throw new AssertionFailedException( "Cannot load two assemblies of the same name (not implemented)." );
            }

            return assembly;
        }

        public override string ToString() => this._domainId.ToString( CultureInfo.InvariantCulture );

        protected virtual void Dispose( bool disposing )
        {
            this._assemblyCache.Clear();

            this._loadedAssemblies.Clear();

            AppDomain.CurrentDomain.AssemblyResolve -= this.OnAssemblyResolve;
        }

        public void Dispose() => this.Dispose( true );
    }
}