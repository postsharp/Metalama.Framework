[TheAspect]
internal class C
{
  public bool CanExecute(object? x) => x != null;
  public void M(global::System.Int32 a)
  {
    if (new global::System.Func<global::System.Object, global::System.Boolean>(parameter => (bool)this.CanExecute((global::System.Int32)parameter)).Invoke(a))
    {
      global::System.Console.WriteLine("Hello, world.");
    }
  }
}