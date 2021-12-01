// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class DateTimeSerializer : ValueTypeMetaSerializer<DateTime>
    {
        public override void SerializeObject( DateTime value, IArgumentsWriter writer )
        {
            writer.SetValue( "k", (byte) value.Kind);
            writer.SetValue("t", value.Ticks);
        }

        public override DateTime DeserializeObject( IArgumentsReader reader )
        {
            return new DateTime(reader.GetValue<long>( "t" ), (DateTimeKind) reader.GetValue<byte>( "k" ));
        }
    }
}