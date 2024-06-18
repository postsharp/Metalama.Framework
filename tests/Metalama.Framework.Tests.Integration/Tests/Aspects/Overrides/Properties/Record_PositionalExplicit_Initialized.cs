using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Record_PositionalExplicit_Initialized;

internal class MyAspect : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get => meta.Proceed();
        set => meta.Proceed();
    }
}

// <target>
internal record MyRecord( int A, int B )
{
    public int A { get; init; } = A;

    private int _b = B;

    public int B
    {
        get
        {
            Console.WriteLine( "Original." );

            return _b;
        }
        init
        {
            Console.WriteLine( "Original." );
        }
    }
}

internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectMany( p => p.Types.OfName( "MyRecord" ).SelectMany( t => t.Properties.Where( p => !p.IsImplicitlyDeclared ) ) )
            .AddAspect<MyAspect>();
    }
}