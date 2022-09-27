[Introduction]
internal class TargetClass
{
  [global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod.FooAttribute]
  [return: global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod.FooAttribute]
  public void IntroducedMethod_Void<
  [global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod.FooAttribute]
  T>([global::Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod.FooAttribute] global::System.Int32 i)
  {
    global::System.Console.WriteLine("This is introduced method.");
  }
}