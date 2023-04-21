public class Target : Base
{
  public override int BaseVirtualOverriddenMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public new virtual int BaseVirtualHiddenMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public new int BaseHiddenMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public new static int BaseStaticHiddenMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public int LocalMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public virtual int LocalVirtualMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public static int LocalStaticMethod
  {
    get
    {
      return 42;
    }
    set
    {
    }
  }
  public int Foo
  {
    get
    {
      // Should invoke this.
      _ = this.BaseMethod;
      // Should invoke this.
      _ = this.BaseMethod;
      // Should invoke this.
      _ = this.BaseMethod;
      // Should invoke this.
      _ = this.BaseMethod;
      // Should invoke current type.
      _ = Target.BaseStaticMethod;
      // Should invoke current type.
      _ = Target.BaseStaticMethod;
      // Should invoke current type.
      _ = Target.BaseStaticMethod;
      // Should invoke current type.
      _ = Target.BaseStaticMethod;
      // Should invoke base.
      _ = this.BaseVirtualMethod;
      // Should invoke base.
      _ = this.BaseVirtualMethod;
      // Should invoke base.
      _ = this.BaseVirtualMethod;
      // Should invoke this.
      _ = this.BaseVirtualMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.BaseVirtualOverriddenMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.BaseVirtualOverriddenMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.BaseVirtualOverriddenMethod;
      // Should invoke this.
      _ = this.BaseVirtualOverriddenMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.BaseVirtualHiddenMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.BaseVirtualHiddenMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.BaseVirtualHiddenMethod;
      // Should invoke this.
      _ = this.BaseVirtualHiddenMethod;
      // Should invoke this.
      _ = this.BaseHiddenMethod;
      // Should invoke this.
      _ = this.BaseHiddenMethod;
      // Should invoke this.
      _ = this.BaseHiddenMethod;
      // Should invoke this.
      _ = this.BaseHiddenMethod;
      // Should invoke current type.
      _ = Target.BaseStaticHiddenMethod;
      // Should invoke current type.
      _ = Target.BaseStaticHiddenMethod;
      // Should invoke current type.
      _ = Target.BaseStaticHiddenMethod;
      // Should invoke current type.
      _ = Target.BaseStaticHiddenMethod;
      // Should invoke this.
      _ = this.LocalMethod;
      // Should invoke this.
      _ = this.LocalMethod;
      // Should invoke this.
      _ = this.LocalMethod;
      // Should invoke this.
      _ = this.LocalMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.LocalVirtualMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.LocalVirtualMethod;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      _ = this.LocalVirtualMethod;
      // Should invoke this.
      _ = this.LocalVirtualMethod;
      // Should invoke current type.
      _ = Target.LocalStaticMethod;
      // Should invoke current type.
      _ = Target.LocalStaticMethod;
      // Should invoke current type.
      _ = Target.LocalStaticMethod;
      // Should invoke current type.
      _ = Target.LocalStaticMethod;
      return 42;
    }
    set
    {
      // Should invoke this.
      this.BaseMethod = value;
      // Should invoke this.
      this.BaseMethod = value;
      // Should invoke this.
      this.BaseMethod = value;
      // Should invoke this.
      this.BaseMethod = value;
      // Should invoke current type.
      Target.BaseStaticMethod = value;
      // Should invoke current type.
      Target.BaseStaticMethod = value;
      // Should invoke current type.
      Target.BaseStaticMethod = value;
      // Should invoke current type.
      Target.BaseStaticMethod = value;
      // Should invoke base.
      this.BaseVirtualMethod = value;
      // Should invoke base.
      this.BaseVirtualMethod = value;
      // Should invoke base.
      this.BaseVirtualMethod = value;
      // Should invoke this.
      this.BaseVirtualMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.BaseVirtualOverriddenMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.BaseVirtualOverriddenMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.BaseVirtualOverriddenMethod = value;
      // Should invoke this.
      this.BaseVirtualOverriddenMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.BaseVirtualHiddenMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.BaseVirtualHiddenMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.BaseVirtualHiddenMethod = value;
      // Should invoke this.
      this.BaseVirtualHiddenMethod = value;
      // Should invoke this.
      this.BaseHiddenMethod = value;
      // Should invoke this.
      this.BaseHiddenMethod = value;
      // Should invoke this.
      this.BaseHiddenMethod = value;
      // Should invoke this.
      this.BaseHiddenMethod = value;
      // Should invoke current type.
      Target.BaseStaticHiddenMethod = value;
      // Should invoke current type.
      Target.BaseStaticHiddenMethod = value;
      // Should invoke current type.
      Target.BaseStaticHiddenMethod = value;
      // Should invoke current type.
      Target.BaseStaticHiddenMethod = value;
      // Should invoke this.
      this.LocalMethod = value;
      // Should invoke this.
      this.LocalMethod = value;
      // Should invoke this.
      this.LocalMethod = value;
      // Should invoke this.
      this.LocalMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.LocalVirtualMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.LocalVirtualMethod = value;
      // Should invoke _Source (#32906 - linker should create _Source declaration and target it).
      this.LocalVirtualMethod = value;
      // Should invoke this.
      this.LocalVirtualMethod = value;
      // Should invoke current type.
      Target.LocalStaticMethod = value;
      // Should invoke current type.
      Target.LocalStaticMethod = value;
      // Should invoke current type.
      Target.LocalStaticMethod = value;
      // Should invoke current type.
      Target.LocalStaticMethod = value;
    }
  }
}