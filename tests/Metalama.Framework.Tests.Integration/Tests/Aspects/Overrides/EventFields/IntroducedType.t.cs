[Aspect]
class Target
{
  class TestType : global::System.Object
  {
    private event global::System.EventHandler? _introducedEvent;
    public event global::System.EventHandler? IntroducedEvent
    {
      add
      {
        global::System.Console.WriteLine("Override");
        this._introducedEvent += value;
      }
      remove
      {
        global::System.Console.WriteLine("Override");
        this._introducedEvent -= value;
      }
    }
  }
}