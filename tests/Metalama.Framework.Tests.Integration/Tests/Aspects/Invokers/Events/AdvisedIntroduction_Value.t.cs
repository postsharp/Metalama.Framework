[TestIntroduction]
[Override]
internal class TargetClass
{
  private event global::System.EventHandler? _event;
  public event global::System.EventHandler? Event
  {
    add
    {
      global::System.Console.WriteLine("Override");
      this._event += value;
    }
    remove
    {
      global::System.Console.WriteLine("Override");
      this._event -= value;
    }
  }
}