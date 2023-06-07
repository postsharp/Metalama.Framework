public partial class TargetClass
{
  [TestAspect]
  public int Foo()
  {
    _ = ((global::System.Int32)global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33273.StaticClass.StaticMethod());
    return 42;
  }
}