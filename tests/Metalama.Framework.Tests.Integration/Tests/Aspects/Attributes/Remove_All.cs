using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Remove;

public class MyAspect : Aspect, IAspect<IDeclaration>
{
    public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }

    public void BuildAspect( IAspectBuilder<IDeclaration> builder )
    {
        builder.RemoveAttributes( GetType() );
    }
}

internal class KeepItAttribute : Attribute { }

#pragma warning disable CS0414, CS1696, CS8618, CS0067

// <target>
[MyAspect]
internal class C
{
    [MyAspect]
    [KeepIt]
    private C() { }

    [MyAspect]
    [KeepIt]
    [return: MyAspect]
    private void M( [MyAspect] int p ) { }

    [MyAspect]
    [KeepIt]
    private int _a = 5, _b = 3;

    [MyAspect]
    [KeepIt]
    private event Action MyEvent1, MyEvent2;

    [MyAspect]
    [KeepIt]
    private event Action MyEvent3;

    [MyAspect]
    private event Action MyEvent4
    {
        add { }
        remove { }
    }

    [MyAspect]
    private class D { }

    [MyAspect]
    private struct S { }
}