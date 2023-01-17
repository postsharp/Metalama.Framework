using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Field;

#pragma warning disable CS8618, CS0169

public class MyAttribute : Attribute { }

public class MyAspect : FieldAspect
{
    public override void BuildAspect( IAspectBuilder<IField> builder )
    {
        builder.Advise.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(MyAttribute) ) );
    }
}

// <target>
internal class C
{
    [MyAspect]
    private int Field1, Field2;

    [MyAspect]
    private int Field3;
}