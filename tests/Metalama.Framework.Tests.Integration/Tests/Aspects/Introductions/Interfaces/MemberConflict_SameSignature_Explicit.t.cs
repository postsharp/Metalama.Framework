[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_SameSignature_Explicit.IInterface
{
    public void Method()
    {
        Console.WriteLine("This is original method.");
    }


    void global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_SameSignature_Explicit.IInterface.Method()
    {
        global::System.Console.WriteLine("This is introduced interface method.");
    }
}