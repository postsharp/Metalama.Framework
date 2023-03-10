// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime.Serialization
{
    /// <summary>
    /// Binds types to names and names to types. Used by the <see cref="CompileTimeSerializer"/>.
    /// </summary>
    internal class CompileTimeSerializationBinder
    {
        private static readonly ImmutableDictionary<string, string> _ourAssemblyVersions;

        private readonly ILogger _logger;

        static CompileTimeSerializationBinder()
        {
            var assemblyNames = typeof(CompileTimeSerializationBinder).Assembly.GetReferencedAssemblies()
                .Concat( typeof(CompileTimeSerializationBinder).Assembly.GetName().AssertNotNull() );

            // The AppDomain may contain several versions of Metalama, so we need to be careful when choosing the assembly version to which we bind.
            // Instead of looking at the AppDomain, we look at the assemblies that the current specific version references. This should work for Metalama
            // and system assemblies. User assemblies are covered by CompileTimeLamaSerializationBinder. 
            _ourAssemblyVersions = assemblyNames.GroupBy( a => a.Name.AssertNotNull() )
                .ToImmutableDictionary( x => x.Key, x => x.OrderByDescending( a => a.Version ).First().ToString() );
        }

        public CompileTimeSerializationBinder( ProjectServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Serialization" );
        }

        /// <summary>
        /// Gets a <see cref="Type"/> given a type name and an assembly name.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The required <see cref="Type"/>.</returns>
        public virtual Type BindToType( string typeName, string assemblyName )
        {
            if ( !_ourAssemblyVersions.TryGetValue( assemblyName, out var ourAssemblyVersion ) )
            {
                if ( !assemblyName.StartsWith( "mscorlib, ", StringComparison.Ordinal ) )
                {
                    this._logger.Warning?.Log( $"'{assemblyName}' is not a known assembly name." );
                }

                ourAssemblyVersion = assemblyName;
            }

            var type = Type.GetType( ReflectionHelper.GetAssemblyQualifiedTypeName( typeName, ourAssemblyVersion ) );

            if ( type == null )
            {
                throw new CompileTimeSerializationException( $"Cannot find the 'type {typeName}, {assemblyName}'." );
            }

            return type;
        }

#pragma warning disable CA1822 // Can be static

        // ReSharper disable once MemberCanBeMadeStatic.Global
        
        /// <summary>
        /// Gets the name and the assembly name of a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <param name="typeName">At output, the name of <paramref name="type"/>.</param>
        /// <param name="assemblyName">At output, the name of <paramref name="assemblyName"/>.</param>
        public void BindToName( Type type, out string typeName, out string assemblyName )
        {
            typeName = type.FullName!;

            // #31016
            // We don't use the full name because it may happen that the graph is serialized in a process that higher
            // assembly versions than the deserializing processes and we don't want, and don't need, to bother with versioning.
            // Versioning and version update, if necessary, should be taken care of upstream, and not by the formatter.
            // When deserializing, we will assume that a compatible assembly version has been loaded in the AppDomain.

            assemblyName = type.Assembly.GetName().Name.AssertNotNull();
        }
    }

#pragma warning restore CA1822 // Can be static    
}