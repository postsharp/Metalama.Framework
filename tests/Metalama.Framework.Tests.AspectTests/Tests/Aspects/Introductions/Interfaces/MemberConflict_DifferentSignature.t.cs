[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.MemberConflict_DifferentSignature.IInterface
{
  public int Method(int x)
  {
    Console.WriteLine("This is original method.");
    return x;
  }
  public global::System.Int32 Method()
  {
    global::System.Console.WriteLine("This is introduced interface method.");
    return (global::System.Int32)42;
  }
}