[Aspect]
internal class Target
{
  class TestType : global::System.Object
  {
    public void IntroducedMethod()
    {
      global::System.Console.WriteLine("Override");
      global::System.Console.WriteLine("Introduced Method");
      return;
    }
  }
}