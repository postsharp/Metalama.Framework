[Override]
[Introduction]
internal class TargetClass : Interface, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember.IntroducedInterface
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
  private EventHandler? _initializerEvent = Foo;
  public event EventHandler? InitializerEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this._initializerEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this._initializerEvent -= value;
    }
  }
  public static void Foo(object? sender, EventArgs args)
  {
  }
  public static void Bar(global::System.Object? sender, global::System.EventArgs args)
  {
  }
  private global::System.EventHandler? _initializerIntroducedEvent = (global::System.EventHandler? )global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember.IntroductionAttribute.Bar;
  public event global::System.EventHandler? InitializerIntroducedEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this._initializerIntroducedEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this._initializerIntroducedEvent -= value;
    }
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
  private global::System.EventHandler? _explicitIntroducedEvent;
  event global::System.EventHandler? global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember.IntroducedInterface.ExplicitIntroducedEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
    }
  }
  private global::System.EventHandler? _initializerExplicitIntroducedEvent = (global::System.EventHandler? )global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember.IntroductionAttribute.Bar;
  event global::System.EventHandler? global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember.IntroducedInterface.InitializerExplicitIntroducedEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      _ = (global::System.EventHandler? )global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember.IntroductionAttribute.Bar;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
    }
  }
}