[Before]
[Override]
[After]
public class Target
{
  private int _autoProperty;
  public int AutoProperty
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this._autoProperty;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this._autoProperty = value;
      return;
    }
  }
  private static int _autoProperty_Static;
  public static int AutoProperty_Static
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target._autoProperty_Static;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target._autoProperty_Static = value;
      return;
    }
  }
  public int AutoProperty_NoOverride { get; set; }
  public static int AutoProperty_Static_NoOverride { get; set; }
  public void Introduced()
  {
    _ = this._autoProperty;
    this._autoProperty = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target._autoProperty_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target._autoProperty_Static = 42;
    _ = this.AutoProperty_NoOverride;
    this.AutoProperty_NoOverride = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static_NoOverride;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static_NoOverride = 42;
  }
  public void IntroducedAfter()
  {
    _ = this.AutoProperty;
    this.AutoProperty = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static = 42;
    _ = this.AutoProperty_NoOverride;
    this.AutoProperty_NoOverride = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static_NoOverride;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static_NoOverride = 42;
  }
  public void IntroducedBefore()
  {
    _ = this._autoProperty;
    this._autoProperty = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target._autoProperty_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target._autoProperty_Static = 42;
    _ = this.AutoProperty_NoOverride;
    this.AutoProperty_NoOverride = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static_NoOverride;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoProperty_FinalInvoker.Target.AutoProperty_Static_NoOverride = 42;
  }
}