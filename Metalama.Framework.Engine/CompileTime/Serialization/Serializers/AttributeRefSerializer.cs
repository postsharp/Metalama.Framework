// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Engine.CompileTime.Serialization.Serializers;

public class AttributeRefSerializer : ReferenceTypeSerializer
{
    public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
    {
        var data = constructorArguments.GetValue<AttributeSerializationData>( "data" ).AssertNotNull();

        return new AttributeRef( data );
    }

    public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
    {
        var attributeRef = (AttributeRef) obj;
        var compilationContext = ((ISerializationContext) constructorArguments).CompilationContext.AssertNotNull();

        if ( !attributeRef.TryGetAttributeSerializationData( compilationContext, out var serializationData ) )
        {
            throw new AssertionFailedException( $"Cannot serialize the attribute '{attributeRef}'." );
        }

        constructorArguments.SetValue( "data", serializationData );
    }

    public override void DeserializeFields( object obj, IArgumentsReader initializationArguments ) { }
}