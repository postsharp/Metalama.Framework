[TheAspect]
internal class C
{
  public C(object o)
  {
  }
  public C(object o, Func<object, bool> f)
  {
  }
  public bool CanExecute(object? x) => x != null;
  public static void Execute1(object o)
  {
  }
  public static void Execute2(Func<object, bool> f) => f(new object ());
  public void M(int a)
  {
    if (new Func<object, bool>(parameter => CanExecute((int)parameter)).Invoke(a))
    {
      Console.WriteLine("Hello, world.");
    }
    Execute1(new Func<object, bool>(parameter => CanExecute((int)parameter)));
    _ = new C(new Func<object, bool>(parameter => CanExecute((int)parameter)));
    var x = new Func<object, bool>(parameter => CanExecute((int)parameter));
    Execute2(parameter => CanExecute((int)parameter));
    _ = new C(null, parameter => CanExecute((int)parameter));
    var y = () => Execute1(new object ());
  }
}