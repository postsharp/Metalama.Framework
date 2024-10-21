using Metalama.Framework.Aspects;
using TestBuildFails;

[assembly: AspectOrder(AspectOrderDirection.RunTime, typeof(Aspect1), typeof(Aspect2), typeof(Aspect3), typeof(Aspect4))]

namespace TestBuildFails;

public class Aspect1 : TypeAspect
{
    [Introduce]
    public void Method1()
    {
    }
}

public class Aspect2 : TypeAspect
{
    [Introduce]
    public void Method2()
    {
    }
}


public class Aspect3 : TypeAspect
{
    [Introduce]
    public void Method3()
    {
    }
}

public class Aspect4 : TypeAspect
{
    [Introduce]
    public void Method4()
    {
    }
}


[Aspect1, Aspect2, Aspect3, Aspect4]
public class TargetClass {}
