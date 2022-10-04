using System.Linq;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue31113;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Method()
    {
        var method = meta.Target.Type.Methods.Single();
        method.Invokers.Final.Invoke( meta.RunTime( method.IsStatic ? null : meta.This ) );
    }
}

[MyAspect]
internal class C
{
    private void M() { }
}