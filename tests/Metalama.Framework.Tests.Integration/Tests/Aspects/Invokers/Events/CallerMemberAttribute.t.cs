[InvokerAspect]
public class TargetClass
{
  public event EventHandler TestEvent
  {
    add
    {
    }
    remove
    {
      OtherClass.Foo();
    }
  }
  [OverrideAspect]
  public event EventHandler TestOverriddenEvent
  {
    add
    {
    }
    remove
    {
      OtherClass.Foo();
    }
  }
  [OverrideAspect]
  public event EventHandler TestOverriddenNonInlinedEvent
  {
    add
    {
      this.TestOverriddenNonInlinedEvent_Source += value;
      this.TestOverriddenNonInlinedEvent_Source += value;
    }
    remove
    {
      this.TestOverriddenNonInlinedEvent_Source -= value;
    }
  }
  private event EventHandler TestOverriddenNonInlinedEvent_Source
  {
    add
    {
    }
    remove
    {
      OtherClass.Foo(callerMemberName: "TestOverriddenNonInlinedEvent");
    }
  }
  public void CallFoo()
  {
    // Invoke this.Property
    this.TestEvent += null;
    // Invoke this.Property
    this.TestEvent += null;
    // Invoke this.Property
    this.TestEvent += null;
    // Invoke this.Property
    this.TestEvent += null;
    // Invoke this.Property
    this.TestOverriddenEvent += null;
    // Invoke this.Property
    this.TestOverriddenEvent += null;
    // Invoke this.Property
    this.TestOverriddenEvent += null;
    // Invoke this.Property
    this.TestOverriddenEvent += null;
    // Invoke this.Property
    this.TestOverriddenNonInlinedEvent += null;
    // Invoke this.Property
    this.TestOverriddenNonInlinedEvent += null;
    // Invoke this.Property
    this.TestOverriddenNonInlinedEvent += null;
    // Invoke this.Property
    this.TestOverriddenNonInlinedEvent += null;
  }
}