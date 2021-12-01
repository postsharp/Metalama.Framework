// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

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
            return new decimal(reader.GetValue<int[]>("d"));
        }
    }
}