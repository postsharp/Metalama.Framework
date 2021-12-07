﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class DateTimeSerializer : ValueTypeMetaSerializer<DateTime>
    {
        public override void SerializeObject( DateTime value, IArgumentsWriter writer )
        {
            writer.SetValue( "k", (byte) value.Kind );
            writer.SetValue( "t", value.Ticks );
        }

        public override DateTime DeserializeObject( IArgumentsReader reader )
        {
            return new DateTime( reader.GetValue<long>( "t" ), (DateTimeKind) reader.GetValue<byte>( "k" ) );
        }
    }
}