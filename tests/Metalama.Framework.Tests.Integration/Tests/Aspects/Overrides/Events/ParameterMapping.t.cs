[Introduction]
internal class TargetClass
{
  public event EventHandler Event
  {
    add
    {
      value.Invoke(null, new global::System.EventArgs());
      EventHandler ev = value;
    }
    remove
    {
      value.Invoke(null, new global::System.EventArgs());
      EventHandler ev = value;
    }
  }
}