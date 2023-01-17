using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Property;

public class MyAttribute : Attribute { }

public class MyAspect : PropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IProperty> builder )
    {
        builder.Advise.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(MyAttribute) ) );

        if (builder.Target.GetMethod != null)
        {
            builder.Advise.IntroduceAttribute( builder.Target.GetMethod, AttributeConstruction.Create( typeof(MyAttribute) ) );
        }

        if (builder.Target.SetMethod != null)
        {
            builder.Advise.IntroduceAttribute( builder.Target.SetMethod, AttributeConstruction.Create( typeof(MyAttribute) ) );
        }
    }
}

// <target>
internal class C
{
    [MyAspect]
    private int Property1 { get; set; }
}