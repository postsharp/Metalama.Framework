[Introduction]
    public class TargetClass:global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.SameMethodName.IInterface    {


public global::System.Int32 Method()
{
    global::System.Console.WriteLine("This is introduced interface method.");
    return (global::System.Int32)0;
}

public global::System.Int32 Method(global::System.Int32 a)
{
    global::System.Console.WriteLine("This is introduced interface method.");
    return (global::System.Int32)a;
}      
    }
