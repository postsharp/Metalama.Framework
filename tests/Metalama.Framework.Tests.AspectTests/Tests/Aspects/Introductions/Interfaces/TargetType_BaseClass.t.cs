[IntroduceAspect]
public class TargetClass : BaseClass, global::Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.TargetType_BaseClass.IInterface
{
  public override void ExistingMethod()
  {
    Console.WriteLine("Original interface member");
  }
  public void IntroducedMethod()
  {
    global::System.Console.WriteLine("Introduced interface member");
  }
}