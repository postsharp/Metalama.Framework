[MyInheritableAspectWhichIntroducesAMethod]
class Foo<T>
{
  public void M()
  {
  }
}
class Bar : Foo<int>
{
  private void CallM()
  {
    this.M();
  }
}