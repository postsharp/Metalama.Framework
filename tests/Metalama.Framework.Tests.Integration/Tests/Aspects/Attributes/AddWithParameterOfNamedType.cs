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
        builder.Advise.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create(
                typeof(MyAttribute),
                constructorArguments: new object?[] { TypeFactory.GetType(typeof(C)) }));
    }
}

// <target>
internal class C
{
    [MyAspect]
    private void M() { }
}