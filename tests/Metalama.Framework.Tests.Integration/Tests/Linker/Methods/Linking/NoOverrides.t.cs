public class Target : Base
{
  public override void BaseVirtualOverriddenMethod()
  {
  }
  public new virtual void BaseVirtualHiddenMethod()
  {
  }
  public new void BaseHiddenMethod()
  {
  }
  public new static void BaseStaticHiddenMethod()
  {
  }
  public void LocalMethod()
  {
  }
  public virtual void LocalVirtualMethod()
  {
  }
  public static void LocalStaticMethod()
  {
  }
  public void Foo()
  {
    // Should invoke this.
    this.BaseMethod();
    // Should invoke this.
    this.BaseMethod();
    // Should invoke this.
    this.BaseMethod();
    // Should invoke this.
    this.BaseMethod();
    // Should invoke current type.
    Target.BaseStaticMethod();
    // Should invoke current type.
    Target.BaseStaticMethod();
    // Should invoke current type.
    Target.BaseStaticMethod();
    // Should invoke current type.
    Target.BaseStaticMethod();
    // Should invoke base.
    this.BaseVirtualMethod();
    // Should invoke base.
    this.BaseVirtualMethod();
    // Should invoke base.
    this.BaseVirtualMethod();
    // Should invoke this.
    this.BaseVirtualMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.BaseVirtualOverriddenMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.BaseVirtualOverriddenMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.BaseVirtualOverriddenMethod();
    // Should invoke this.
    this.BaseVirtualOverriddenMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.BaseVirtualHiddenMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.BaseVirtualHiddenMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.BaseVirtualHiddenMethod();
    // Should invoke this.
    this.BaseVirtualHiddenMethod();
    // Should invoke this.
    this.BaseHiddenMethod();
    // Should invoke this.
    this.BaseHiddenMethod();
    // Should invoke this.
    this.BaseHiddenMethod();
    // Should invoke this.
    this.BaseHiddenMethod();
    // Should invoke current type.
    Target.BaseStaticHiddenMethod();
    // Should invoke current type.
    Target.BaseStaticHiddenMethod();
    // Should invoke current type.
    Target.BaseStaticHiddenMethod();
    // Should invoke current type.
    Target.BaseStaticHiddenMethod();
    // Should invoke this.
    this.LocalMethod();
    // Should invoke this.
    this.LocalMethod();
    // Should invoke this.
    this.LocalMethod();
    // Should invoke this.
    this.LocalMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.LocalVirtualMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.LocalVirtualMethod();
    // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
    this.LocalVirtualMethod();
    // Should invoke this.
    this.LocalVirtualMethod();
    // Should invoke current type.
    Target.LocalStaticMethod();
    // Should invoke current type.
    Target.LocalStaticMethod();
    // Should invoke current type.
    Target.LocalStaticMethod();
    // Should invoke current type.
    Target.LocalStaticMethod();
  }
}