// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers;

internal sealed class RefSerializer<T> : ReferenceTypeSerializer<IRef>
    where T : class, ICompilationElement
{
    public override IRef CreateInstance( IArgumentsReader constructorArguments )
    {
        var id = constructorArguments.GetValue<string>( "id" ).AssertNotNull();

        return ((ISerializationContext) constructorArguments).CompilationContext.RefFactory.FromDeclarationId<T>( new SerializableDeclarationId( id ) );
    }

    public override void SerializeObject( IRef obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
    {
        constructorArguments.SetValue( "id", obj.ToSerializableId().Id );
    }

    public override void DeserializeFields( IRef obj, IArgumentsReader initializationArguments ) { }
}