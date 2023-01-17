using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32222;

public class MyAspect : OverrideMethodAspect
{
    private string _tag;

    public MyAspect( string tag )
    {
        _tag = tag;
    }

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine(this._tag);

        return meta.Proceed();
    }
}


// <target>
internal class C
{
    [MyAspect("Direct")]
    private void M() { }

    class Fabric : TypeFabric
    {
        public override void AmendType( ITypeAmender amender )
        {
            amender.Amend.SelectMany( t=>t.Methods ).AddAspect<MyAspect>( _ => new MyAspect( "Fabric" ));
        }
    }
}