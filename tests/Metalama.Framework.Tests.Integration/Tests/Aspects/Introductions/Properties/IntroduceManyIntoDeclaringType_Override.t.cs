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
  private global::System.String Property
  {
    get
    {
      global::System.Console.WriteLine($"Metalama.Framework.Tests.Integration.Aspects.Introductions.Properties.IntroduceManyIntoDeclaringType_Override.TargetCode.M2() says hello.");
      global::System.Console.WriteLine($"Metalama.Framework.Tests.Integration.Aspects.Introductions.Properties.IntroduceManyIntoDeclaringType_Override.TargetCode.M() says hello.");
      return default(global::System.String);
    }
  }
}