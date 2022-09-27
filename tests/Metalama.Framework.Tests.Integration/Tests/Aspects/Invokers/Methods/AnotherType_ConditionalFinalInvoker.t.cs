internal class TargetClass
{
  [TestAttribute]
  public void VoidMethod(OtherClass other)
  {
    other?.VoidMethod();
    return;
  }
  [TestAttribute]
  public int? Method(OtherClass other, int? x)
  {
    return other?.Method(x);
  }
}