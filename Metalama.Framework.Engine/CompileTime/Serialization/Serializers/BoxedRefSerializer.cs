// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers;

internal sealed class BoxedRefSerializer<T> : ReferenceTypeSerializer<BoxedRef<T>>
    where T : class, ICompilationElement
{
    public override BoxedRef<T> CreateInstance( IArgumentsReader constructorArguments )
    {
        var id = constructorArguments.GetValue<string>( "id" )
                 ?? throw new AssertionFailedException();

        return new BoxedRef<T>( Ref.FromDeclarationId<IDeclaration>( new SerializableDeclarationId( id ) ) );
    }

    public override void SerializeObject( BoxedRef<T> obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
    {
        if ( !obj.IsDefault )
        {
            constructorArguments.SetValue( "id", obj.ToSerializableId().Id );
        }
    }

    public override void DeserializeFields( BoxedRef<T> obj, IArgumentsReader initializationArguments ) { }
}