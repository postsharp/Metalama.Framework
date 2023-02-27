// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

// Resharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Metalama.Framework.Engine.CompileTime
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
        private readonly ILogger _logger;
        private readonly object _sync = new();
        private readonly ConcurrentDictionary<string, (Assembly Assembly, AssemblyIdentity Identity)> _assembliesByName = new();
        private ImmutableDictionaryOfArray<string, string> _assemblyPathsByName = ImmutableDictionaryOfArray<string, string>.Empty;

        protected ICompileTimeDomainObserver? Observer { get; }

        public CompileTimeDomain( GlobalServiceProvider serviceProvider )
        {
            this.Observer = serviceProvider.GetService<ICompileTimeDomainObserver>();
            this.Observer?.OnDomainCreated( this );

            AppDomain.CurrentDomain.AssemblyResolve += this.OnAssemblyResolve;
            this._logger = Logger.Domain;
        }

        private Assembly? OnAssemblyResolve( object? sender, ResolveEventArgs args )
        {
            this._logger.Trace?.Log( $"Resolving the assembly '{args.Name}' requested by '{args.RequestingAssembly}'." );

            var assemblyName = new AssemblyName( args.Name );

            if ( this._assembliesByName.TryGetValue( assemblyName.Name.AssertNotNull(), out var candidateAssembly )
                 && AssemblyName.ReferenceMatchesDefinition( assemblyName, candidateAssembly.Assembly.GetName() ) )
            {
                this._logger.Trace?.Log( $"Found the assembly '{candidateAssembly.Assembly.Location}'." );

                return candidateAssembly.Assembly;
            }
            else
            {
                var matchingAssemblies = this._assemblyPathsByName[assemblyName.Name.AssertNotNull()]
                    .Select( x => (Path: x, AssemblyName: AssemblyName.GetAssemblyName( x )) )
                    .Where( x => AssemblyName.ReferenceMatchesDefinition( assemblyName, x.AssemblyName ) )
                    .ToOrderedList( x => x.AssemblyName.Version, descending: true );

                if ( matchingAssemblies.Count >= 1 )
                {
                    this._logger.Trace?.Log( $"Found the assembly '{matchingAssemblies[0].Path}'." );

                    return this.LoadAssembly( matchingAssemblies[0].Path );
                }
            }

            this._logger.Warning?.Log( $"Could not find the assembly '{args.Name}'." );

            return null;
        }

        // ReSharper disable once VirtualMemberNeverOverridden.Global, 

        /// <summary>
        /// Loads an assembly in the CLR. The default implementation is compatible with the .NET Framework,
        /// but it can be overwritten for .NET Core.
        /// </summary>
        [PublicAPI] // Overridden by Metalama.Try.
        public virtual Assembly LoadAssembly( string path )
        {
            try
            {
                return Assembly.LoadFile( path );
            }
            catch ( Exception e )
            {
                throw new FileLoadException( $"Cannot load '{path}': {e.Message}", e );
            }
        }

        /// <summary>
        /// Gets an assembly given its <see cref="AssemblyIdentity"/> and image, or loads it.
        /// </summary>
        internal Assembly GetOrLoadAssembly( AssemblyIdentity compileTimeIdentity, string path )
        {
            var assembly = this._assemblyCache.GetOrAdd(
                compileTimeIdentity,
                _ =>
                {
                    this._logger.Trace?.Log( $"Loading assembly '{path}'." );

                    return this.LoadAssembly( path );
                } );

            // CompileTimeDomain is used only for compile-time assemblies, which always have a unique name, so we can have safely
            // index assemblies by name only.
            this._assembliesByName.AddOrUpdate(
                compileTimeIdentity.Name,
                _ => (assembly, compileTimeIdentity),
                ( name, value ) =>
                {
                    if ( !value.Identity.Equals( compileTimeIdentity ) )
                    {
                        throw new AssertionFailedException( $"Cannot load two assemblies of the same name '{name}'." );
                    }

                    return value;
                } );

            return assembly;
        }

        public override string ToString() => this._domainId.ToString( CultureInfo.InvariantCulture );

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                this._assemblyCache.Clear();

                this._assembliesByName.Clear();

                AppDomain.CurrentDomain.AssemblyResolve -= this.OnAssemblyResolve;
            }
        }

        public void Dispose() => this.Dispose( true );

        internal void RegisterAssemblyPaths( ImmutableArray<string> systemAssemblyPaths )
        {
            lock ( this._sync )
            {
                this._assemblyPathsByName = this._assemblyPathsByName.AddRange( systemAssemblyPaths, x => Path.GetFileNameWithoutExtension( x ), x => x );
            }
        }
    }
}