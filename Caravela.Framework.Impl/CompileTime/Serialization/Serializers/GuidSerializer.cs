// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Serialization;
using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization.Serializers
{
    internal sealed class GuidSerializer : ValueTypeMetaSerializer<Guid>
    {
        public override void SerializeObject( Guid value, IArgumentsWriter writer )
        {
            writer.SetValue( "g", value.ToByteArray() );
        }

        public override Guid DeserializeObject( IArgumentsReader reader )
        {
            return new Guid( reader.GetValue<byte[]>( "g" ) );
        }
    }
}