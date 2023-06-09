public partial class TargetClass : BaseClass
{
  public override void Foo()
  {
    this.Foo_Source();
  }
  private void Foo_Source()
  {
    Console.WriteLine("Override method with no aspect override");
  }
  [TestAspect]
  public void Bar()
  {
    this.Foo_Source();
    return;
  }
}