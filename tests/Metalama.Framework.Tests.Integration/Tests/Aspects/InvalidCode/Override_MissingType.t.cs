// Final Compilation.Emit failed.
// Error CS0103 on `Foo`: `The name 'Foo' does not exist in the current context`
class TargetCode
{
  [Aspect]
  void Method(int a)
  {
    global::System.Console.WriteLine("Aspect");
    Foo.Bar();
    return;
  }
}