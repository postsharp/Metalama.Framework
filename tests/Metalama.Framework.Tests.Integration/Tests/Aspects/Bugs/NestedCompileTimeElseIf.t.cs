[Aspect(I = -1)]
class TargetM1
{
  private void M()
  {
    global::System.Console.WriteLine(-1);
    global::System.Console.WriteLine("I <= 0");
  }
}
[Aspect(I = 1)]
class Target1
{
  private void M()
  {
    global::System.Console.WriteLine(1);
  }
}