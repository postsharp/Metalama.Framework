using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.SelectAttributes_Inheritable;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectDeclarationsWithAttribute<Marker>()
            .OfType<INamedType>()
            .AddAspectIfEligible<TheAspect>(
                m =>
                {
                    var attribute = m.Attributes.GetConstructedAttributesOfType<Marker>().Single();

                    return new TheAspect( attribute );
                } );
    }
}

[RunTimeOrCompileTime]
public class Marker : Attribute, ICompileTimeSerializable
{
    public Marker( string value )
    {
        Value = value;
    }

    public string Value { get; }
}

[Inheritable]
public class TheAspect : TypeAspect
{
    private readonly Marker? _marker;

    public TheAspect( Marker marker )
    {
        _marker = marker;
    }

    [Introduce( WhenExists = OverrideStrategy.Override )]
    public virtual void Introduced()
    {
        Console.WriteLine( $"Marker: {_marker.Value}" );
    }
}

[Marker( "The Marker" )]
public class BaseClass { }