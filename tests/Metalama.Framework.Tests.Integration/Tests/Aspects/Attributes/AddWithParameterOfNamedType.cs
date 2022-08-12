using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.AddWithParameterOfNamedType;

public class MyAttribute : Attribute 
{
    public MyAttribute(Type t)
    {
    }
}

public class MyAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.IntroduceAttribute(
            builder.Target, 
            AttributeConstruction.Create(
                typeof(MyAttribute),
                constructorArguments: new object?[] { builder.Target.DeclaringType }));
    }
}

// <target>
internal class C
{
    [MyAspect]
    private void M() { }
}