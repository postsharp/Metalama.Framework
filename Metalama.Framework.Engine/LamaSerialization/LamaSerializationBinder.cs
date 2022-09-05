// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.LamaSerialization
{
    /// <summary>
    /// Binds types to names and names to types. Used by the <see cref="LamaFormatter"/>.
    /// </summary>
    internal class LamaSerializationBinder
    {
        private readonly ImmutableDictionary<string, string> _preferredAssemblies;
        private readonly ILogger _logger;

        public LamaSerializationBinder( IServiceProvider serviceProvider )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Serialization" );

            var assemblyNames = this.GetType().Assembly.GetReferencedAssemblies().Concat( this.GetType().Assembly.GetName() );

            this._preferredAssemblies = assemblyNames.GroupBy( a => a.Name )
                .ToImmutableDictionary( x => x.Key, x => x.OrderByDescending( a => a.Version ).First().ToString() );
        }

        /// <summary>
        /// Gets a <see cref="Type"/> given a type name and an assembly name.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>The required <see cref="Type"/>.</returns>
        public virtual Type BindToType( string typeName, string assemblyName )
        {
            if ( !this._preferredAssemblies.TryGetValue( assemblyName, out var preferredAssemblyName ) )
            {
                this._logger.Warning?.Log( $"'{assemblyName}' is not a known assembly name." );
                preferredAssemblyName = assemblyName;
            }

            var type = Type.GetType( ReflectionHelper.GetAssemblyQualifiedTypeName( typeName, preferredAssemblyName ) );

            if ( type == null )
            {
                throw new LamaSerializationException( $"Cannot find the 'type {typeName}, {assemblyName}'." );
            }

            return type;
        }

        /// <summary>
        /// Gets the name and the assembly name of a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <param name="typeName">At output, the name of <paramref name="type"/>.</param>
        /// <param name="assemblyName">At output, the name of <paramref name="assemblyName"/>.</param>
        public virtual void BindToName( Type type, out string typeName, out string assemblyName )
        {
            typeName = type.FullName!;

            // #31016
            // We don't use the full name because it may happen that the graph is serialized in a process that higher
            // assembly versions than the deserializing processes and we don't want, and don't need, to bother with versioning.
            // Versioning and version update, if necessary, should be taken care of upstream, and not by the formatter.
            // When deserializing, we will assume that a compatible assembly version has been loaded in the AppDomain.

            assemblyName = type.Assembly.GetName().Name;
        }
    }
}