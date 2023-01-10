public class Target
{
  event EventHandler? Foo
  {
    add
    {
      Console.WriteLine("Before");
      this.Foo_Source?.Invoke(null, new EventArgs());
      Console.WriteLine("After");
    }
    remove
    {
    }
  }
  private event EventHandler? Foo_Source;
}