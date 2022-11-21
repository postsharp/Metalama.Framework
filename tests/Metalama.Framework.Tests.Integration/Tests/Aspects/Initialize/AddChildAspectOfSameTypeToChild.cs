using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Initialize.AddChildAspectOfSameTypeToChild;

[AttributeUsage( AttributeTargets.Class )]
public class MyAspect : OverrideMethodAspect, IAspect<INamedType>
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "Oops" );

        return meta.Proceed();
    }

    public void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.With( t => t.Methods ).AddAspectIfEligible<MyAspect>( _ => this );
    }

    public void BuildEligibility( IEligibilityBuilder<INamedType> builder ) { }
}

// <target>
[MyAspect]
internal class C
{
    private void M() { }
}