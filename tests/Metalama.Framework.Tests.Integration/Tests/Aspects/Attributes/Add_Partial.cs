using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Partial;

[Inherited]
public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(SerializableAttribute) ) );
    }
}

// <target>
[MyAspect]
internal partial class C { }

// <target>
internal partial class C { }