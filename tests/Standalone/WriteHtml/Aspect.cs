using Metalama.Framework.Aspects;

public class MyAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Overridden.");
        return meta.Proceed();

    }
}