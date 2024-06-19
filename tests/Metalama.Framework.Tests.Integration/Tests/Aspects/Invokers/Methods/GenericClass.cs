using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Introduced()
    {
        meta.Target.Type.Methods.OfName( "Method" ).Single().Invoke();
        meta.Target.Type.WithTypeArguments( typeof(int) ).Methods.OfName( "Method" ).Single().Invoke();
    }
}

// <target>
[MyAspect]
public class C<T>
{
    public static void Method() { }
}