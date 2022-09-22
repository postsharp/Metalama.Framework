class Target
{
  public int Foo
  {
    get
    {
      return this.Foo_Source4;
    }
    set
    {
      this.Foo_Source4 = value;
    }
  }
  private int Foo_Source4 { get; set; }
  public int _foo { get; set; }
  public int Foo_Source { get; set; }
  public int _foo1()
  {
    return 42;
  }
  public int Foo_Source1()
  {
    return 42;
  }
  public event EventHandler? _foo2;
  public event EventHandler? Foo_Source2;
  public int _foo3;
  public int Foo_Source3;
}