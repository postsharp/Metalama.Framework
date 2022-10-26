internal class BaseClass
{
  [InheritedAspect]
  public virtual void ClassMethodWithAspect()
  {
    global::System.Console.WriteLine("Hacked!");
    return;
  }
  public virtual void ClassMethodWithoutAspect()
  {
  }
}
internal interface IInterface
{
  [InheritedAspect]
  private void InterfaceMethodWithAspect()
  {
    global::System.Console.WriteLine("Hacked!");
    return;
  }
  private void InterfaceMethodWithoutAspect()
  {
  }
}
internal class DerivedClass : BaseClass, IInterface
{
  public override void ClassMethodWithAspect()
  {
    global::System.Console.WriteLine("Hacked!");
    base.ClassMethodWithAspect();
    return;
  }
  public override void ClassMethodWithoutAspect()
  {
    base.ClassMethodWithoutAspect();
  }
  public virtual void InterfaceMethodWithAspect()
  {
    global::System.Console.WriteLine("Hacked!");
    return;
  }
  public virtual void InterfaceMethodWithoutAspect()
  {
  }
}
internal class DerivedTwiceClass : DerivedClass
{
  public override void ClassMethodWithAspect()
  {
    global::System.Console.WriteLine("Hacked!");
    base.ClassMethodWithAspect();
    return;
  }
  public override void ClassMethodWithoutAspect()
  {
    base.ClassMethodWithoutAspect();
  }
  public override void InterfaceMethodWithAspect()
  {
    global::System.Console.WriteLine("Hacked!");
    base.InterfaceMethodWithAspect();
    return;
  }
  public override void InterfaceMethodWithoutAspect()
  {
    base.InterfaceMethodWithoutAspect();
  }
}