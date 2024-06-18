using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Record_PositionalExplicit;

internal class MyAspect : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get => meta.Proceed();
        set => meta.Proceed();
    }
}

#pragma warning disable CS8907 // Parameter is unread. Did you forget to use it to initialize the property with that name?

// <target>
internal record MyRecord( int A, int B )
{
    public int A { get; init; }

    public int B
    {
        get
        {
            Console.WriteLine( "Original." );

            return 42;
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