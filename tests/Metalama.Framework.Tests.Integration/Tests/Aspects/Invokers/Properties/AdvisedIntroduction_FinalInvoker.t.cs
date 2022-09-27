[Introduction]
[Override]
internal class TargetClass
{
  public global::System.Int32 Property
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this.Property;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this.Property = value;
    }
  }
}