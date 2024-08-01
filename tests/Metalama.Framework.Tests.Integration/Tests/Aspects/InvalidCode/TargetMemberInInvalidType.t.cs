// Syntax tree verification failed.
// Error CS1031 on ``: `Type expected`
internal partial class InvalidBase : SomethingThatDoesNotExist
{
  [Test]
  public void Foo()
  {
    global::System.Console.WriteLine("Aspect");
    return;
  }
}
internal partial class MissingInterface : object, {
  [Test]
  public void Foo()
  {
    global::System.Console.WriteLine("Aspect");
    return;
  }
}
internal partial class InvalidInterface : object, ISomethingThatDoesNotExist
{
  [Test]
  public void Foo()
  {
    global::System.Console.WriteLine("Aspect");
    return;
  }
}
internal partial class InvalidTypeParameterList<T, >
{
  [Test]
  public void Foo()
  {
    global::System.Console.WriteLine("Aspect");
    return;
  }
}