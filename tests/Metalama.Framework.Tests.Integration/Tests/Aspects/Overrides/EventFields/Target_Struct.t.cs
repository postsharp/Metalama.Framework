[Override]
[Introduction]
internal struct TargetClass
{
  private event EventHandler? _event;
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
  private static event EventHandler? _staticEvent;
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
  public TargetClass()
  {
  }
  private event global::System.EventHandler? _introducedEvent;
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
  private static event global::System.EventHandler? _introducedStaticEvent;
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