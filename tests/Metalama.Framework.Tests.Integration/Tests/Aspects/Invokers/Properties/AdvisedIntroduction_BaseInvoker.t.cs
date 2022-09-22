[Introduction]
[Override]
internal class TargetClass
{
  public global::System.Int32 Property
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this.Property_Source;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this.Property_Source = value;
    }
  }
  private global::System.Int32 Property_Source { get; set; }
}