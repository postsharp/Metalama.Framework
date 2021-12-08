// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class TimeSpanSerializer : ValueTypeMetaSerializer<TimeSpan>
    {
        public override void SerializeObject( TimeSpan value, IArgumentsWriter writer )
        {
            writer.SetValue( "t", value.Ticks );
        }

        public override TimeSpan DeserializeObject( IArgumentsReader reader )
        {
            return new TimeSpan( reader.GetValue<long>( "t" ) );
        }
    }
}