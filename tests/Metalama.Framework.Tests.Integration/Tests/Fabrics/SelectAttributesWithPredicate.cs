using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.SelectAttributesWithPredicate;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectDeclarationsWithAttribute<Marker>( a => a.Value )
            .OfType<IMethod>()
            .AddAspectIfEligible<TheAspect>(
                m =>
                {
                    return new TheAspect();
                } );
    }
}

[RunTimeOrCompileTime]
public class Marker : Attribute
{
    public Marker( bool value )
    {
        Value = value;
    }

    public bool Value { get;  }
}

public class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( $"Marked!" );

        return meta.Proceed();
    }
}

// <target>
[Marker(true)]
public class C
{
    [Marker( false )]
    public void UnmarkedMethod() { }

    [Marker( true )]
    public void MarkedMethod() { }
}