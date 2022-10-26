using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.Accessors;

public class MyAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "Overridden." );

        return meta.Proceed();
    }
}

// <target>
internal class C
{
    public int Property1
    {
        [MyAspect]
        get;
    }

    public int Property2
    {
        [MyAspect]
        get;
        [MyAspect]
        set;
    }

    public event Action Event1
    {
        [MyAspect]
        add { }

        [MyAspect]
        remove { }
    }

    public event Action Event2
    {
        [MyAspect]
        add { }

        remove { }
    }
}