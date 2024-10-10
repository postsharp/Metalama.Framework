using App;
using Lib;
using Metalama.Framework.Aspects;
[assembly: AspectOrder(typeof(Aspect1), typeof(Aspect2), typeof(RedistributedAspect))]

namespace App;



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

[Aspect1, Aspect2]
public partial class TheClass
{
}