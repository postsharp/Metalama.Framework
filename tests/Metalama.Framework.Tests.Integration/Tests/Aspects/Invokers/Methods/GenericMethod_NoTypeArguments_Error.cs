using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.GenericMethod_NoTypeArguments;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Introduced()
    {
        meta.Target.Type.Methods.OfName("Method").Single().WithTypeArguments(typeof(int)).Invoke();

        var otherType = meta.Target.Type.ContainingNamespace.Types.OfName("D").Single();

        otherType.Methods.OfName("Method").Single().WithTypeArguments(typeof(int)).Invoke();
    }
}

// <target>
[MyAspect]
public class C
{
    public static void Method<T>() { }
}

// <target>
public class D
{
    public static void Method<T>() { }
}