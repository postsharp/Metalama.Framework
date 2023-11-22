using System;
using System.Linq;
using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Options.ProjectFabric_OutOfOrder;

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        // Test that when we set project-level options last, they do not override leaf-level options.
        amender.Outbound.Select( c => c.Types.OfName( nameof(C2) ).Single() ).SetOptions( c => new MyOptions { Value = "C2" } );

        amender.Outbound.Select( c => c.Types.OfName( nameof(C2) ).Single() )
            .Select( t => t.Methods.OfName( nameof(C2.M2) ).Single() )
            .SetOptions( c => new MyOptions { Value = "M2" } );

        amender.Outbound.SetOptions( c => new MyOptions { Value = "Project" } );
    }
}

// <target>
[ShowOptionsAspect]
public class C1
{
    [ShowOptionsAspect]
    public void M( [ShowOptionsAspect] int p ) { }
}

// <target>
[ShowOptionsAspect]
public class C2
{
    [ShowOptionsAspect]
    public void M( [ShowOptionsAspect] int p ) { }

    [ShowOptionsAspect]
    public void M2( [ShowOptionsAspect] int p ) { }

    [ShowOptionsAspect]
    public int P
    {
        [ShowOptionsAspect]
        get;
        [ShowOptionsAspect]
        set;
    }

    [ShowOptionsAspect]
    public int F;

    [ShowOptionsAspect]
    public event EventHandler? E;

    public class N
    {
        [ShowOptionsAspect]
        public void M( [ShowOptionsAspect] int p ) { }
    }
}