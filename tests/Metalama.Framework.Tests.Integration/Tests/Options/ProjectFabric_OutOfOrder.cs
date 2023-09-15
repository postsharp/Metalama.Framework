using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.ProjectFabric_OutOfOrder;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        // Test that when we set project-level options last, they do not override leaf-level options.
        amender.Outbound.Select( c => c.Types.OfName( nameof(C2) ).Single() ).Configure( c => new MyOptions { Value = "C2" } );

        amender.Outbound.Select( c => c.Types.OfName( nameof(C2) ).Single() )
            .Select( t => t.Methods.OfName( nameof(C2.M2) ).Single() )
            .Configure( c => new MyOptions { Value = "M2" } );

        amender.Outbound.Configure( c => new MyOptions { Value = "Project" } );
    }
}

// <target>
[OptionsAspect]
public class C1
{
    [OptionsAspect]
    public void M( [OptionsAspect] int p ) { }
}

// <target>
[OptionsAspect]
public class C2
{
    [OptionsAspect]
    public void M( [OptionsAspect] int p ) { }

    [OptionsAspect]
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
    public event EventHandler E;

    public class N
    {
        [OptionsAspect]
        public void M( [OptionsAspect] int p ) { }
    }
}