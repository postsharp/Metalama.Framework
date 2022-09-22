[Introduction]
[Test]
internal class TargetClass : BaseClass
{
  public int TargetClassProperty
  {
    get
    {
      global::System.Console.WriteLine("This is introduced property.");
      return this.TargetClassProperty;
    }
  }
  public override global::System.Int32 BaseClassProperty
  {
    get
    {
      global::System.Console.WriteLine("This is introduced property.");
      return base.BaseClassProperty;
    }
  }
}