internal class TargetClass
{
  [Test]
  public int Property
  {
    get
    {
      return this.Property_Source;
    }
    set
    {
      this.Property_Source = value;
    }
  }
  private int Property_Source { get; set; }
}