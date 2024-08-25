using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.GenericClass_NoTypeArguments_Error;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Introduced()
    {
        var otherType = meta.Target.Type.ContainingNamespace.Types.OfName("D").Single();

        // Method in another type should not be callable without specifying type arguments.
        otherType.Methods.OfName("Method").Single().Invoke();
    }
}

// <target>
[MyAspect]
public class C
{
}

public class D<U>
{
    public static void Method() { }
}

