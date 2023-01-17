using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Type;

[Inheritable]
public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(SerializableAttribute) ) );
    }
}

// <target>
internal class Output
{
    [MyAspect]
    internal class C { }

    internal class D : C { }
}

// <target>
internal record TargetRecord
{
    [MyAspect]
    internal record C { }

    internal record D : C { }
}

// <target>
internal record class TargetRecordClass
{
    [MyAspect]
    internal record class C { }

    internal record class D : C { }
}