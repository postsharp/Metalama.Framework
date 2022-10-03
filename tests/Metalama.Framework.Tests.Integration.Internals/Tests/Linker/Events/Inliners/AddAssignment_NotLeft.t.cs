public class Target
{
  event EventHandler? Foo
  {
    add
    {
      Console.WriteLine("Before");
      EventHandler? x = null;
      x += this.Foo_Source;
      Console.WriteLine("After");
    }
    remove
    {
    }
  }
  private EventHandler? Foo_Source;
}