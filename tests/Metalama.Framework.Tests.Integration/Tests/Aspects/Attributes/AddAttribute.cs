using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.AddAttribute;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddAttribute( builder.Target, AttributeConstruction.Create( typeof(SerializableAttribute) ) );
    }
}

[MyAspect]
internal class C
{
    
}