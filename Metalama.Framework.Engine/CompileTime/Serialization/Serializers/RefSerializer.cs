// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers;

internal sealed class RefSerializer<T> : ReferenceTypeSerializer<BaseRef<T>>
    where T : class, ICompilationElement
{
    public override BaseRef<T> CreateInstance( IArgumentsReader constructorArguments )
    {
        var id = constructorArguments.GetValue<string>( "id" ).AssertNotNull();

        return
            (BaseRef<T>)
            ((ISerializationContext) constructorArguments).CompilationContext.RefFactory.FromDeclarationId<T>( new SerializableDeclarationId( id ) );
    }

    public override void SerializeObject( BaseRef<T> obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
    {
        constructorArguments.SetValue( "id", obj.ToSerializableId().Id );
    }

    public override void DeserializeFields( BaseRef<T> obj, IArgumentsReader initializationArguments ) { }
}