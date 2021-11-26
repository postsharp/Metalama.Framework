// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Utilities.Dump
{
    public sealed class ObjectDumper
    {
        public ImmutableArray<Property> Properties { get; }

        private static readonly ConcurrentDictionary<Type, ObjectDumper> _instances = new();

        public record Property( string PropertyName, MethodInfo Getter, bool IsLazy );

        private ObjectDumper( Type type )
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
                .Select( g => new Property( PropertyName: g.Name.Substring( 4 ), g, IsLazy: RequiresLazyLoad( g.ReturnType ) ) )
                .OrderBy( p => (p.PropertyName, p.Getter.ReturnType), PropertyComparer.Instance )
                .ToImmutableArray();
        }

        internal static bool RequiresLazyLoad( Type propertyType ) => typeof(IEnumerable).IsAssignableFrom( propertyType ) && propertyType != typeof(string);

        private static bool IsPublicType( Type type ) => type.IsPublic && type.Assembly != typeof(ObjectDumper).Assembly;

        public static IDumpFormatter Formatter { get; set; } = new LinqPadFormatter();

        private object? DumpCore( object? obj )
        {
            switch ( obj )
            {
                case null:
                    return null;

                case IReadOnlyCollection<byte> { Count: > 1024 } bytes:
                    return $"{bytes.Count} bytes";

                case IReadOnlyCollection<byte> bytes:
                    return string.Join( "", bytes.Select( b => b.ToString( "x2", CultureInfo.InvariantCulture ) ) );
            }

            IDictionary<string, object?> values = new ExpandoObject();

            foreach ( var property in this.Properties )
            {
                object? value;

                try
                {
                    value = property.Getter.Invoke( obj, null );

                    /*
                    var isLazy = false;

                    if ( property.IsLazy )
                    {
                        isLazy = true;
                    }
                    else
                    {
                        value = property.Getter.Invoke( obj, null );

                        if ( value is ICollection { Count: > 100 } )
                        {
                            isLazy = true;
                        }
                    }

                    if ( isLazy )
                    {
                        value = Formatter.FormatLazyPropertyValue( obj, property.Getter );
                    }
                    else
                    {
                        value = Formatter.FormatPropertyValue( value!, property.Getter );
                    }
                    */

                    value = Formatter.FormatPropertyValue( value!, property.Getter );
                }
                catch ( Exception e )
                {
                    value = Formatter.FormatException( e );
                }

                values.Add( property.PropertyName, value );
            }

            return values;
        }

        public static object? Dump( object? obj ) => obj == null ? null : GetDumper( obj.GetType() ).DumpCore( obj );

        public static ObjectDumper GetDumper( Type type ) => _instances.GetOrAdd( type, t => new ObjectDumper( t ) );

        public class PropertyComparer : IComparer<(string Name, Type Type)>
        {
            private PropertyComparer() { }

            public static readonly PropertyComparer Instance = new();

            private static int GetPropertyNamePriority( string s )
                => s switch
                {
                    "Index" => 0,
                    "Position" => 1,
                    "Name" => 2,
                    _ => 10
                };

            private static int GetPropertyTypePriority( Type t ) => t.GetInterface( "IEnumerable`1" ) != null ? 1 : 0;

            public int Compare( (string Name, Type Type) x, (string Name, Type Type) y )
            {
                var priorityComparison = GetPropertyNamePriority( x.Name ).CompareTo( GetPropertyNamePriority( y.Name ) );

                if ( priorityComparison != 0 )
                {
                    return priorityComparison;
                }

                var typeComparison = GetPropertyTypePriority( x.Type ).CompareTo( GetPropertyTypePriority( y.Type ) );

                if ( typeComparison != 0 )
                {
                    return typeComparison;
                }

                return StringComparer.Ordinal.Compare( x.Name, y.Name );
            }
        }
    }
}