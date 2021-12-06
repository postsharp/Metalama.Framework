// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class DecimalSerializer : ValueTypeMetaSerializer<decimal>
    {
        public override void SerializeObject( decimal value, IArgumentsWriter writer )
        {
            writer.SetValue( "d", decimal.GetBits( value ) );
        }

        public override decimal DeserializeObject( IArgumentsReader reader )
        {
            return new decimal( reader.GetValue<int[]>( "d" ) );
        }
    }
}