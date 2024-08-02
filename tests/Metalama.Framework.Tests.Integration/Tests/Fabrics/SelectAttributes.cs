using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.SelectAttributes;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectDeclarationsWithAttribute<Marker>()
            .OfType<IMethod>()
            .AddAspectIfEligible<TheAspect>(
                m =>
                {
                    var attribute = m.Attributes.OfAttributeType<Marker>().Single();

                    return new TheAspect( attribute.Value );
                } );
    }
}

[RunTimeOrCompileTime]
public class Marker : Attribute
{
    public string? Value { get; set; }
}

public class DerivedMarker : Marker;

public class TheAspect : OverrideMethodAspect
{
    private readonly string? _marker;

    public TheAspect( string? marker )
    {
        _marker = marker;
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( $"Marker: {_marker}" );

        return meta.Proceed();
    }
}

// <target>
[Marker]
public class C
{
    public void UnmarkedMethod() { }

    [Marker( Value = "TheMarker" )]
    public void MarkedMethod() { }

    [DerivedMarker( Value = "DerivedMarker")]
    public void DerivedMethod() { }
}