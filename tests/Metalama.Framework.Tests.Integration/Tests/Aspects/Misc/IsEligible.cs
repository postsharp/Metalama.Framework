using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.IsEligible;

public class MyAspect : OverrideMethodAspect
{
    public override void BuildEligibility( IEligibilityBuilder<IMethod> builder )
    {
        base.BuildEligibility( builder );
        builder.MustBeNonStatic();
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "Overridden." );

        return default;
    }
}

public class TopLevelAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.With( t => t.Methods.Where( m => m.IsEligible<MyAspect>() ) ).AddAspect<MyAspect>();
    }
}

// <target>
[TopLevelAspect]
internal class C
{
    public void EligibleMethod() { }

    public static void NonEligibleMethod() { }
}