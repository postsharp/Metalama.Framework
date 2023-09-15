using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Options.AspectModifyingProjectFabric;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.Outbound.Configure( c => new MyOptions { Value = "FromFabric.Project" } );
        amender.Outbound.Select( c => c.Types.OfName( nameof(C2) ).Single() ).Configure( c => new MyOptions { Value = "FromFabric.C2" } );

        amender.Outbound.Select( c => c.Types.OfName( nameof(C2) ).Single() )
            .Select( t => t.Methods.OfName( nameof(C2.M2) ).Single() )
            .Configure( c => new MyOptions { Value = "FromFabric.M2" } );
    }
}

// <target>
[OptionsAspect]
[ModifyOptionsAspect( "FromAspect.C1" )]
public class C1
{
    [OptionsAspect]
    public void M( [OptionsAspect] int p ) { }
}

// <target>
[OptionsAspect]
[ModifyOptionsAspect( "FromAspect.C2" )]
public class C2
{
    [OptionsAspect]
    public void M( [OptionsAspect] int p ) { }

    [OptionsAspect]
    [ModifyOptionsAspect( "FromAspect.P" )]
    public void M2( [OptionsAspect] int p ) { }

    [OptionsAspect]
    public int P
    {
        [OptionsAspect]
        get;
        [OptionsAspect]
        set;
    }

    [OptionsAspect]
    public int F;

    [OptionsAspect]
    public event EventHandler? E;

    public class N
    {
        [OptionsAspect]
        public void M( [OptionsAspect] int p ) { }
    }
}