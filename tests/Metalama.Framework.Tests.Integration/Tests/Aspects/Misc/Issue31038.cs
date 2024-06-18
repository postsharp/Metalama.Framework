using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue31038;

public class MyAspect : TypeAspect
{
    [Introduce]
    public void Method()
    {
        Console.WriteLine( $"PI={Math.PI}" );
    }
}

// <target>
[MyAspect]
public class C { }