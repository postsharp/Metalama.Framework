﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;
using System.Linq;
using Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.SecondaryInstancesOrdering;

// Tests that secondary instances are sorted propertly according to the declaration depth.

[assembly: MyAspect( "Assembly" )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.SecondaryInstancesOrdering;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly | AttributeTargets.Method, AllowMultiple = false )]
public class MyAspect : OverrideMethodAspect, IAspect<ICompilation>, IAspect<INamedType>
{
    private string _tag;

    public MyAspect( string tag )
    {
        _tag = tag;
    }

    public void BuildAspect( IAspectBuilder<ICompilation> builder )
    {
        builder.Outbound.SelectMany( c => c.Types.SelectMany( t => t.Methods ) ).AddAspectIfEligible( _ => this );
    }

    public void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Outbound.SelectMany( t => t.Methods ).AddAspect( _ => this );
    }

    public void BuildEligibility( IEligibilityBuilder<ICompilation> builder ) { }

    public void BuildEligibility( IEligibilityBuilder<INamedType> builder ) { }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(
            $"Aspect order: {_tag}, {string.Join( ", ", meta.AspectInstance.SecondaryInstances.Select( x => ( (MyAspect)x.Aspect )._tag ) )}" );

        return meta.Proceed();
    }
}

// <target>
[MyAspect( "Type" )]
internal class C
{
    [MyAspect( "Method" )]
    private void M() { }
}