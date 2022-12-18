[Override]
[Introduction]
internal class TargetClass
{
  private EventHandler? _event;
  public event EventHandler? Event
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this._event += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this._event -= value;
    }
  }
  private static EventHandler? _staticEvent;
  public static event EventHandler? StaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members.TargetClass._staticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members.TargetClass._staticEvent -= value;
    }
  }
  public void Foo()
  {
    this._event?.Invoke(this, new EventArgs());
    _staticEvent?.Invoke(this, new EventArgs());
  }
  public void Bar()
  {
    this._introducedEvent?.Invoke(this, new global::System.EventArgs());
    global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members.TargetClass._introducedStaticEvent?.Invoke(this, new global::System.EventArgs());
  }
  private global::System.EventHandler? _introducedEvent;
  public event global::System.EventHandler? IntroducedEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this._introducedEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this._introducedEvent -= value;
    }
  }
  private static global::System.EventHandler? _introducedStaticEvent;
  public static event global::System.EventHandler? IntroducedStaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members.TargetClass._introducedStaticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members.TargetClass._introducedStaticEvent -= value;
    }
  }
}