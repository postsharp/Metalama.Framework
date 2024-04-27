using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Params;

public class MyAttribute : Attribute
{
    public MyAttribute( string a, params int[] p ) { }
}

[Inheritable]
public class MyAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        var attr = AttributeConstruction.Create( typeof(MyAttribute), new object[] { "x", 1, 2, 3, 4, 5 } );
        builder.Advice.IntroduceAttribute( builder.Target, attr );
    }
}

// <target>
internal class C
{
    [MyAspect]
    private void M() { }
}