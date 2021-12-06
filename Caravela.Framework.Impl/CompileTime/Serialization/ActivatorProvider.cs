// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Provides instances of classes implementing the <see cref="IMetaActivator"/> interface. You should not use this class in user code.
    /// </summary>
    internal sealed class ActivatorProvider
    {
        private readonly object _sync = new();
        private readonly Dictionary<Assembly, IMetaActivator?> _assemblyActivators = new();
        private readonly Dictionary<Type, IMetaActivator?> _typeActivators = new();

        internal ActivatorProvider() { }

        /// <summary>
        /// Gets an instance of a given class implementing the <see cref="IMetaActivator"/> interface.
        /// </summary>
        /// <param name="type">A type implementing the <see cref="IMetaActivator"/> interface.</param>
        /// <returns>An instance of type <paramref name="type"/>.</returns>
        public IMetaActivator? GetActivator( Type type )
        {
            lock ( this._sync )
            {
                if ( this._typeActivators.TryGetValue( type, out var activator ) )
                {
                    return activator;
                }

                var requiredAssembly = new Assembly[1];

                ReflectionHelper.VisitTypeElements(
                    type,
                    t =>
                    {
                        if ( !t.IsPrimitive && !t.HasElementType && !(t.IsGenericType && !t.IsGenericTypeDefinition) &&
                             !ReflectionHelper.IsPublic( t ) )
                        {
                            if ( requiredAssembly[0] != null && requiredAssembly[0] != t.Assembly )
                            {
                                throw new MetaSerializationException(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        "Cannot instantiate type '{0}' because there is no assembly from which the type is fully accessible.",
                                        type ) );
                            }

                            requiredAssembly[0] = t.Assembly;
                        }
                    } );

                if ( requiredAssembly[0] != null && requiredAssembly[0] != this.GetType().Assembly )
                {
                    activator = this.GetActivator( requiredAssembly[0] );

                    if ( activator == null )
                    {
                        throw new MetaSerializationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Cannot instantiate type '{0}' because assembly '{1}' does not have an IActivator.",
                                type,
                                requiredAssembly[0] ) );
                    }
                }
                else
                {
                    activator = null;
                }

                this._typeActivators.Add( type, activator );

                return activator;
            }
        }

        private IMetaActivator? GetActivator( Assembly assembly )
        {
            if ( this._assemblyActivators.TryGetValue( assembly, out var activator ) )
            {
                return activator;
            }

            var attributes = assembly.GetCustomAttributes( typeof(MetaActivatorTypeAttribute), false );

            if ( attributes.Length > 0 )
            {
                activator = (IMetaActivator) Activator.CreateInstance( ((MetaActivatorTypeAttribute) attributes[0]).ActivatorType );
            }
            else
            {
                activator = null;
            }

            this._assemblyActivators.Add( assembly, activator );

            return activator;
        }
    }
}