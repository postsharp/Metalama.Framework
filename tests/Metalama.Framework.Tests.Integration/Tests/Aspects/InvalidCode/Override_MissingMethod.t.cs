// Final Compilation.Emit failed.
// Error CS0103 on `Bar`: `The name 'Bar' does not exist in the current context`
class TargetCode
{
  [Aspect]
  void Method(int a)
  {
    global::System.Console.WriteLine("Aspect");
    Bar();
    return;
  }
}