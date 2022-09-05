﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.LamaSerialization.Serializers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal sealed class BuiltInSerializerFactoryProvider : SerializerFactoryProvider
    {
        public BuiltInSerializerFactoryProvider( IServiceProvider serviceProvider )
            : base( serviceProvider, new ReflectionSerializationProvider( serviceProvider ) )
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
            this.AddSerializer( typeof(ImmutableDictionary<,>), typeof(ImmutableDictionarySerializer<,>) );
            this.AddSerializer( typeof(ImmutableArray<>), typeof(ImmutableArraySerializer<>) );

            // Our own types.
            this.AddSerializer( typeof(Ref<>), typeof(RefSerializer<>) );
            this.AddSerializer( typeof(DeclarationSerializableId), typeof(DeclarationSerializableIdSerializer) );

            this.MakeReadOnly();
        }
    }
}