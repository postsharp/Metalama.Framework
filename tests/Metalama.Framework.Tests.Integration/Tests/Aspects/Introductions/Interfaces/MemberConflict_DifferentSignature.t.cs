[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_DifferentSignature.IInterface
{
    public int Method(int x)
    {
        Console.WriteLine("This is original method.");
        return x;
    }


    public void Method()
    {
        global::System.Console.WriteLine("This is introduced interface method.");
    }
}