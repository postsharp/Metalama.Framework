class Target
{
  [TestAspect]
  public void Foo<T>()
  {
    this.Bar<T>((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33685.TestData<T>)new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33685.TestData<T>());
    return;
  }
  public void Bar<T>(TestData<T> data)
  {
  }
}