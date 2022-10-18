// The compiled assembly contains dynamic code.
[Override]
[Introduction]
internal class TargetClass
{
  private EventHandler? _event = Foo;
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
  private static EventHandler? _staticEvent = Foo;
  public static event EventHandler? StaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Initializers.TargetClass._staticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Initializers.TargetClass._staticEvent -= value;
    }
  }
  private static void Foo(object? sender, EventArgs args)
  {
  }
  private global::System.EventHandler? _introducedEvent = (global::System.EventHandler? )global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Initializers.TargetClass.Foo;
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
  private static global::System.EventHandler? _introducedStaticEvent = (global::System.EventHandler? )global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Initializers.TargetClass.Foo;
  public static event global::System.EventHandler? IntroducedStaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Initializers.TargetClass._introducedStaticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Initializers.TargetClass._introducedStaticEvent -= value;
    }
  }
}