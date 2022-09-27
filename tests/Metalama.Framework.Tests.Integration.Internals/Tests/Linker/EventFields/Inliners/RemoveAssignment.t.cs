class Target
{
  private EventHandler? _foo;
  event EventHandler? Foo
  {
    add
    {
    }
    remove
    {
      Console.WriteLine("Before");
      this._foo -= value;
      Console.WriteLine("After");
    }
  }
}