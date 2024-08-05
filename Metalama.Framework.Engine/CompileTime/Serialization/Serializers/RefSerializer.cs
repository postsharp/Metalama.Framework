// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers;

internal sealed class RefSerializer<T> : ValueTypeSerializer<Ref<T>>
    where T : class, ICompilationElement
{
    public override void SerializeObject( Ref<T> obj, IArgumentsWriter constructorArguments )
    {
        if ( !obj.IsDefault )
        {
            constructorArguments.SetValue( "id", obj.ToSerializableId().Id );
        }
    }

    public override Ref<T> DeserializeObject( IArgumentsReader constructorArguments )
    {
        var id = constructorArguments.GetValue<string>( "id" );

        if ( id == null )
        {
            return default;
        }

        return Ref.FromDeclarationId<T>( new SerializableDeclarationId( id ) );
    }
}