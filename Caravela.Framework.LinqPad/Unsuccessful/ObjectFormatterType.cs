// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities.Dump;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.LinqPad
{
    internal class ObjectFormatterType
    {
        public IEnumerable<string> PropertyNames { get; }

        public IEnumerable<Type> PropertyTypes { get; }

        public ImmutableArray<Property> Properties { get; }

        private static readonly ConcurrentDictionary<Type, ObjectFormatterType> _instances = new();

        public record Property( string PropertyName, MethodInfo Getter, bool IsLazy );

        private ObjectFormatterType( Type type )
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
                .OrderBy( p => p.PropertyName )
                .ToImmutableArray();

            this.PropertyNames = this.Properties.Select( p => p.PropertyName ).ToImmutableArray();
            this.PropertyTypes = this.Properties.Select( p => p.Getter.ReturnType ).ToImmutableArray();
        }

        internal static bool RequiresLazyLoad( Type propertyType ) => typeof(IEnumerable).IsAssignableFrom( propertyType ) && propertyType != typeof(string);

        private static bool IsPublicType( Type type ) => type.IsPublic && type.Assembly != typeof(ObjectFormatterType).Assembly;

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
                object? value = null;

                try
                {
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
                }
                catch ( Exception e )
                {
                    value = Formatter.FormatException( e );
                }

                values.Add( property.PropertyName, value );
            }

            return values;
        }

        public static object? Dump( object? obj ) => obj == null ? null : GetFormatterType( obj.GetType() ).DumpCore( obj );

        public static ObjectFormatterType GetFormatterType( Type type ) => _instances.GetOrAdd( type, t => new ObjectFormatterType( t ) );
    }
}