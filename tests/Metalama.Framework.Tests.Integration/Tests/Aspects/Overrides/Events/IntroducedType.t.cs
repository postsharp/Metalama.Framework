[Aspect]
internal class Target
{
  class TestType : global::System.Object
  {
    public event global::System.EventHandler IntroducedEvent
    {
      add
      {
        global::System.Console.WriteLine("Override");
        global::System.Console.WriteLine("Introduced");
      }
      remove
      {
        global::System.Console.WriteLine("Override");
        global::System.Console.WriteLine("Introduced");
      }
    }
  }
}