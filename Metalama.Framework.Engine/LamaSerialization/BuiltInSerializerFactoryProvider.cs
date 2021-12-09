// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.LamaSerialization.Serializers;
using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal sealed class BuiltInSerializerFactoryProvider : SerializerFactoryProvider
    {
        public BuiltInSerializerFactoryProvider()
            : base( new ReflectionSerializationProvider() )
        {
            // intrinsic types
            this.AddSerializer<bool, BooleanSerializer>();
            this.AddSerializer<byte, ByteSerializer>();
            this.AddSerializer<char, CharSerializer>();
            this.AddSerializer<short, Int16Serializer>();
            this.AddSerializer<int, Int32Serializer>();
            this.AddSerializer<long, Int64Serializer>();
            this.AddSerializer<ushort, UInt16Serializer>();
            this.AddSerializer<uint, UInt32Serializer>();
            this.AddSerializer<ulong, UInt64Serializer>();
            this.AddSerializer<sbyte, SByteSerializer>();
            this.AddSerializer<float, SingleSerializer>();
            this.AddSerializer<decimal, DecimalSerializer>();
            this.AddSerializer<string, StringSerializer>();
            this.AddSerializer<double, DoubleSerializer>();

            this.AddSerializer<DateTime, DateTimeSerializer>();
            this.AddSerializer<TimeSpan, TimeSpanSerializer>();
            this.AddSerializer<DottedString, DottedStringSerializer>();
            this.AddSerializer<Guid, GuidSerializer>();
            this.AddSerializer<CultureInfo, CultureInfoSerializer>();

            // collections
            this.AddSerializer( typeof(List<>), typeof(ListSerializer<>) );
            this.AddSerializer( typeof(Dictionary<,>), typeof(DictionarySerializer<,>) );
            this.MakeReadOnly();
        }

        public override Type GetSurrogateType( Type objectType )
        {
            return GetAbstractReflectionType( objectType ) ?? objectType;
        }

        public override ISerializerFactory? GetSerializerFactory( Type objectType )
        {
            return base.GetSerializerFactory( GetAbstractReflectionType( objectType ) ?? objectType );
        }

        public static Type? GetAbstractReflectionType( Type type )
        {
            if ( typeof(MethodInfo).IsAssignableFrom( type ) )
            {
                return typeof(MethodInfo);
            }

            if ( typeof(ConstructorInfo).IsAssignableFrom( type ) )
            {
                return typeof(ConstructorInfo);
            }

            if ( typeof(FieldInfo).IsAssignableFrom( type ) )
            {
                return typeof(FieldInfo);
            }

            if ( typeof(Assembly).IsAssignableFrom( type ) )
            {
                return typeof(Assembly);
            }

            if ( typeof(EventInfo).IsAssignableFrom( type ) )
            {
                return typeof(EventInfo);
            }

            if ( typeof(ParameterInfo).IsAssignableFrom( type ) )
            {
                return typeof(ParameterInfo);
            }

            if ( typeof(PropertyInfo).IsAssignableFrom( type ) )
            {
                return typeof(PropertyInfo);
            }

            return null;
        }
    }
}