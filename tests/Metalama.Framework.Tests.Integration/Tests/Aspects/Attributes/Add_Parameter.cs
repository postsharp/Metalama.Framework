using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Parameter;

public class MyAttribute : Attribute { }

[Inheritable]
public class MyAspect : ParameterAspect
{
    public override void BuildAspect( IAspectBuilder<IParameter> builder )
    {
        builder.Advice.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(MyAttribute) ) );
    }
}

// <target>
internal class C
{
    private void M( [MyAspect] int p ) { }
}