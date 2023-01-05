// Warning CS0414 on `_event`: `The field 'TargetClass._event' is assigned but its value is never used`
// Warning CS0414 on `_staticEvent`: `The field 'TargetClass._staticEvent' is assigned but its value is never used`
// Warning CS0414 on `_introducedEvent`: `The field 'TargetClass._introducedEvent' is assigned but its value is never used`
// Warning CS0414 on `_introducedStaticEvent`: `The field 'TargetClass._introducedStaticEvent' is assigned but its value is never used`
[Override]
[Introduction]
internal struct TargetClass
{
  public TargetClass()
  {
  }
  private event EventHandler? _event = default;
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
  private static event EventHandler? _staticEvent = default;
  public static event EventHandler? StaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Target_Struct.TargetClass._staticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Target_Struct.TargetClass._staticEvent -= value;
    }
  }
  private event global::System.EventHandler? _introducedEvent = default;
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
  private static event global::System.EventHandler? _introducedStaticEvent = default;
  public static event global::System.EventHandler? IntroducedStaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Target_Struct.TargetClass._introducedStaticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Target_Struct.TargetClass._introducedStaticEvent -= value;
    }
  }
}