using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

#pragma warning disable CS0414, CS1696

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Remove;

public class MyAspect : Aspect, IAspect<IDeclaration>
{
    public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }

    public void BuildAspect( IAspectBuilder<IDeclaration> builder )
    {
        builder.Advice.RemoveAttributes( builder.Target, GetType() );
    }
}

// <target>
[MyAspect]
internal class C
{
    [MyAspect]
    private C() { }

    [MyAspect]
    private void M( [MyAspect] int p ) { }

    [MyAspect]
    private int _a = 5, _b = 3;

    [MyAspect]
    private event Action MyEvent;

    [MyAspect]
    private event Action MyEvent2
    {
        add { }
        remove { }
    }

    [MyAspect]
    private class D { }

    [MyAspect]
    private struct S { }
}