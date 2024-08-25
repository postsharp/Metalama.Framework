using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.GenericClass;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Introduced()
    {

        // Method in this type should be callable without specifying arguments.
        meta.Target.Type.Methods.OfName( "Method" ).Single().Invoke();

        // Method in this type should be callable while using own type parameter as type argument.
        meta.Target.Type.WithTypeArguments(meta.Target.Type.TypeParameters[0]).Methods.OfName("Method").Single().Invoke();

        // Method in this type should be callable while using another type as argument.
        meta.Target.Type.WithTypeArguments(typeof(int)).Methods.OfName("Method").Single().Invoke();

        var otherType = meta.Target.Type.ContainingNamespace.Types.OfName("D").Single();

        // Method in another type should be callable while using own type parameter as type argument.
        otherType.WithTypeArguments(meta.Target.Type.TypeParameters[0]).Methods.OfName("Method").Single().Invoke();

        // Method in another type should be callable while using another type as argument.
        otherType.WithTypeArguments(typeof(int)).Methods.OfName("Method").Single().Invoke();
    }
}

// <target>
[MyAspect]
public class C<T>
{
    public static void Method() { }
}

public class D<U>
{
    public static void Method() { }
}
