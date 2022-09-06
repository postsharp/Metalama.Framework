// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.LamaSerialization.Serializers;

internal class DeclarationSerializableIdSerializer : ValueTypeSerializer<DeclarationSerializableId>
{
    public override void SerializeObject( DeclarationSerializableId obj, IArgumentsWriter constructorArguments )
        => constructorArguments.SetValue( "id", obj.Id );

    public override DeclarationSerializableId DeserializeObject( IArgumentsReader constructorArguments )
    {
        var id = constructorArguments.GetValue<string>( "id" ).AssertNotNull();

        return new DeclarationSerializableId( id );
    }
}