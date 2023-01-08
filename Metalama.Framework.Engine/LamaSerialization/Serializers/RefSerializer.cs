// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.LamaSerialization.Serializers;

internal sealed class RefSerializer<T> : ValueTypeSerializer<Ref<T>>
    where T : class, ICompilationElement
{
    public override void SerializeObject( Ref<T> obj, IArgumentsWriter constructorArguments )
        => constructorArguments.SetValue( "id", obj.ToSerializableId().Id );

    public override Ref<T> DeserializeObject( IArgumentsReader constructorArguments )
    {
        var id = constructorArguments.GetValue<string>( "id" ).AssertNotNull();

        return Ref.FromSerializedId<T>( new SerializableDeclarationId( id ) );
    }
}