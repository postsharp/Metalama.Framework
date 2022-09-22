internal class TargetCode
{
  [Aspect]
  private void M()
  {
  }
  [Aspect]
  private void M2()
  {
  }
  private void NewMethod()
  {
    global::System.Console.WriteLine($"Metalama.Framework.Tests.Integration.Aspects.Introductions.Methods.IntroduceManyIntoDeclaringType_Override.TargetCode.M2() says hello.");
    global::System.Console.WriteLine($"Metalama.Framework.Tests.Integration.Aspects.Introductions.Methods.IntroduceManyIntoDeclaringType_Override.TargetCode.M() says hello.");
  }
}