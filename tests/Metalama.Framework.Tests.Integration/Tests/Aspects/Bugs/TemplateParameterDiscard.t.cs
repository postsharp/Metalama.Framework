internal class Program
{
  [Aspect]
  void M(int arg)
  {
    _ = arg;
  }
}