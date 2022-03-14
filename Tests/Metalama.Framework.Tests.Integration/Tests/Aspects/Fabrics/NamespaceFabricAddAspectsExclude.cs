using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Fabrics.NamespaceFabricAddAspectsExclude;

internal class Fabric : NamespaceFabric
{
    public override void AmendNamespace( INamespaceAmender amender )
    {
        amender
            .WithTargetMembers(
                c => c.AllTypes
                    .SelectMany( t => t.Methods )
                    .Where( m => m.ReturnType.Is( typeof(string) ) ) )
            .AddAspect<Aspect>();
    }
}

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "overridden" );

        return meta.Proceed();
    }
}

internal class TargetCode
{
    private int Method1( int a )
    {
        return a;
    }

    private string Method2( string s )
    {
        return s;
    }
}

[ExcludeAspect( typeof(Aspect) )]
internal class ExcludedCode
{
    private int Method1( int a )
    {
        return a;
    }

    private string Method2( string s )
    {
        return s;
    }
}