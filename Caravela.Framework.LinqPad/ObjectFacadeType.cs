// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Caravela.Framework.LinqPad
{
    internal class ObjectFacadeType
    {
        public IEnumerable<string> PropertyNames { get; }

        public IEnumerable<Type> PropertyTypes { get; }

        public ImmutableArray<ObjectFacadeProperty> Properties { get; }

        private static readonly ConcurrentDictionary<Type, ObjectFacadeType> _instances = new();

        private ObjectFacadeType( Type type )
        {
            Dictionary<string, PropertyInfo> publicProperties = new();

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
                        var property =
                            getter.DeclaringType!.GetProperty(
                                getter.Name.Substring( 4 ),
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

                        if ( property != null )
                        {
                            publicProperties[property.Name] = property;
                        }
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

                    publicProperties[property.Name] = property;
                }
            }

            var facadeProperties = publicProperties
                .Select( g => new ObjectFacadeProperty( Name: g.Key, g.Value.PropertyType, CreateGetFunc( g.Value ), false ) )
                .OrderBy( p => (p.Name, p.Type), PropertyComparer.Instance )
                .ToList();

            if ( typeof(IDeclaration).IsAssignableFrom( type ) )
            {
                facadeProperties.Add( new ObjectFacadeProperty( "Permalink", typeof(Permalink), o => new Permalink( (IDeclaration) o ), false ) );
            }

            this.Properties = facadeProperties.ToImmutableArray();

            this.PropertyNames = this.Properties.Select( p => p.Name ).ToImmutableArray();
            this.PropertyTypes = this.Properties.Select( p => p.Type ).ToImmutableArray();
        }

        private static Func<object, object?> CreateGetFunc( PropertyInfo property )
        {
            var parameter = Expression.Parameter( typeof(object) );
            var castParameter = Expression.Convert( parameter, property.DeclaringType! );
            var getProperty = Expression.Convert( Expression.Property( castParameter, property ), typeof(object) );
            var lambda = Expression.Lambda<Func<object, object?>>( getProperty, parameter ).Compile();

            return lambda;
        }

        private static bool IsPublicType( Type type ) => type.IsPublic && type.Assembly != typeof(ObjectFacadeType).Assembly;

        public static ObjectFacadeType GetFormatterType( Type type ) => _instances.GetOrAdd( type, t => new ObjectFacadeType( t ) );
    }
}