public class Target
{
  event EventHandler? Foo
  {
    add
    {
    }
    remove
    {
      Console.WriteLine("Before");
      this.Foo_Source?.Invoke(null, new EventArgs());
      Console.WriteLine("After");
    }
  }
  private event EventHandler? Foo_Source;
}