[Override]
[Introduction]
internal class TargetClass
{
  public event EventHandler? Event
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this.Event_Source += value;
      this.Event_Source += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this.Event_Source -= value;
      this.Event_Source -= value;
    }
  }
  private EventHandler? Event_Source;
  public static event EventHandler? StaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.StaticEvent_Source += value;
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.StaticEvent_Source += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.StaticEvent_Source -= value;
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.StaticEvent_Source -= value;
    }
  }
  private static EventHandler? StaticEvent_Source;
  public void Foo()
  {
    if (this.Event_Source != null)
    {
      this.Event_Source(this, new EventArgs());
    }
    if (StaticEvent_Source != null)
    {
      StaticEvent_Source(this, new EventArgs());
    }
  }
  public void Bar()
  {
    if (this.IntroducedEvent_Source != null)
    {
      this.IntroducedEvent_Source(this, new global::System.EventArgs());
    }
    if (global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.IntroducedStaticEvent_Source != null)
    {
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.IntroducedStaticEvent_Source(this, new global::System.EventArgs());
    }
  }
  public event global::System.EventHandler? IntroducedEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this.IntroducedEvent_Source += value;
      this.IntroducedEvent_Source += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this.IntroducedEvent_Source -= value;
      this.IntroducedEvent_Source -= value;
    }
  }
  private global::System.EventHandler? IntroducedEvent_Source;
  public static event global::System.EventHandler? IntroducedStaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.IntroducedStaticEvent_Source += value;
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.IntroducedStaticEvent_Source += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.IntroducedStaticEvent_Source -= value;
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Uninlineable.TargetClass.IntroducedStaticEvent_Source -= value;
    }
  }
  private static global::System.EventHandler? IntroducedStaticEvent_Source;
}