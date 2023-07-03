class TargetCode
{
  [Aspect, Aspect]
  int Method(int a)
  {
    global::System.Console.WriteLine("1 other instance(s)");
    return a;
  }
}