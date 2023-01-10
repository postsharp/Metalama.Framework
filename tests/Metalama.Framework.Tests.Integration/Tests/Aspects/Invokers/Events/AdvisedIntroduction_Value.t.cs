[TestIntroduction]
[Override]
internal class TargetClass
{
  public event global::System.EventHandler? Event
  {
    add
    {
      global::System.Console.WriteLine("Override");
      this.Event_Source += value;
    }
    remove
    {
      global::System.Console.WriteLine("Override");
      this.Event_Source -= value;
    }
  }
  private event global::System.EventHandler? Event_Source;
}