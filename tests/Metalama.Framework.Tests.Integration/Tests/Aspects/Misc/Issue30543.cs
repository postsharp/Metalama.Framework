using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        var typesReceiver = amender.With(
            proj => proj.Types
                .Where( type => IsTypeEligible( type ) ) );

        var propertiesReceiver = amender.With(
            proj => proj.Types
                .Where( type => IsTypeEligible( type ) )
                .SelectMany( type => type.AllProperties.Where( prop => IsPropertyEligible( prop ) ) ) );

        //typesReceiver.AddAspect<ChangeTrackingTypesAspect>();
        propertiesReceiver.AddAspect<PropOverride>();
    }

    private bool IsTypeEligible( INamedType type )
    {
        return type.Is( typeof(TestClass) );
    }

    private bool IsPropertyEligible( IProperty property )
    {
        return true;
    }
}

public class PropOverride : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get
        {
            return meta.Proceed()?.ToUpper();
        }
        set
        {
// Here we are trying different kinds of compile-time statements.
            if (1 == 1)
            {
                meta.Proceed();
            }

            if (1 == 1)
            {
                _ = meta.Proceed();
            }

            if (1 == 1)
            {
                meta.InsertComment( "x" );
            }

            if (1 == 1)
            {
                if (meta.Target.Declaration != null)
                {
                    meta.Proceed();
                }
            }
        }
    }
}

public class TestClass
{
    public string Prop132 { get; set; } = "";
}

internal class Program
{
    private static void Main()
    {
        var widget = new TestClass();

        widget.Prop132 = "aaa";

        Console.WriteLine( widget.Prop132 );
    }
}