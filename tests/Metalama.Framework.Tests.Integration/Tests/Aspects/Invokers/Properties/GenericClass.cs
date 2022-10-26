using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.GenericClass;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Introduced()
    {
        meta.Target.Type.Methods.OfName( "Method" ).Single().Invokers.Final.Invoke( null );
        meta.Target.Type.WithTypeArguments( typeof(int) ).Methods.OfName( "Method" ).Single().Invokers.Final.Invoke( null );
    }
}

// <target>
[MyAspect]
public class C<T>
{
    public static void Method() { }
}