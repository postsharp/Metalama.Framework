[TheAspect]
internal class C
{
  public bool CanExecute(object? x) => x != null;
  public static void Execute(Func<object, bool> f) => f(new object ());
  public void M(int a)
  {
    if (new Func<object, bool>(parameter => CanExecute((int)parameter)).Invoke(a))
    {
      Console.WriteLine("Hello, world.");
    }
    C.Execute(parameter => this.CanExecute((int)parameter));
  }
}