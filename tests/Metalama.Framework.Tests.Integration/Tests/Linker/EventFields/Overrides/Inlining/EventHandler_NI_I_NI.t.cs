class Target
{
  event EventHandler? Foo
  {
    add
    {
      this.Foo_Override2 += value;
    }
    remove
    {
      this.Foo_Override2 -= value;
    }
  }
  private event EventHandler? Foo_Source;
  event EventHandler? Foo_Override2
  {
    add
    {
      Console.WriteLine("Before2");
      Console.WriteLine("Before1");
      this.Foo_Source += value;
      Console.WriteLine("After1");
      Console.WriteLine("After2");
    }
    remove
    {
      Console.WriteLine("Before2");
      Console.WriteLine("Before1");
      this.Foo_Source -= value;
      Console.WriteLine("After1");
      Console.WriteLine("After2");
    }
  }
}