// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.LinqPad
{
    internal class FormattedObjectType
    {
        public IEnumerable<string> PropertyNames { get; }

        public IEnumerable<Type> PropertyTypes { get; }

        public ImmutableArray<FormattedObjectProperty> Properties { get; }

        private static readonly ConcurrentDictionary<Type, FormattedObjectType> _instances = new();

        private FormattedObjectType( Type type )
        {
            HashSet<MethodInfo> publicGetters = new();

            // Find getters of properties of public interfaces.
            foreach ( var implementedInterface in type.GetInterfaces() )
            {
                if ( !IsPublicType( implementedInterface ) )
                {
                    continue;
                }

                var interfaceMap = type.GetInterfaceMap( implementedInterface );

                foreach ( var getter in interfaceMap.TargetMethods )
                {
                    if ( getter.Name.StartsWith( "get_", StringComparison.Ordinal ) && getter.GetParameters().Length == 0 )
                    {
                        publicGetters.Add( getter );
                    }
                }
            }

            // Find getters of public properties.
            if ( IsPublicType( type ) )
            {
                foreach ( var property in type.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
                {
                    var getter = property.GetMethod;

                    if ( getter == null || getter.GetParameters().Length != 0 )
                    {
                        continue;
                    }

                    publicGetters.Add( getter );
                }
            }

            /*
            // Find properties with [Memo].
            var memoGetters = type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                .Where( p => p.IsDefined( typeof(MemoAttribute) ) )
                .Select( p => p.GetMethod )
                .ToImmutableHashSet();
*/

            this.Properties = publicGetters
                .Select( g => new FormattedObjectProperty( PropertyName: g.Name.Substring( 4 ), g, false ) )
                .OrderBy( p => (p.PropertyName, p.Getter.ReturnType), PropertyComparer.Instance )
                .ToImmutableArray();

            this.PropertyNames = this.Properties.Select( p => p.PropertyName ).ToImmutableArray();
            this.PropertyTypes = this.Properties.Select( p => p.Getter.ReturnType ).ToImmutableArray();
        }

        private static bool IsPublicType( Type type ) => type.IsPublic && type.Assembly != typeof(FormattedObjectType).Assembly;

        public static FormattedObjectType GetFormatterType( Type type ) => _instances.GetOrAdd( type, t => new FormattedObjectType( t ) );
    }
}