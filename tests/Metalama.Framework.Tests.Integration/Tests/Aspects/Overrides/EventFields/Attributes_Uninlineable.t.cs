[Introduction]
[Override]
internal class TargetClass
{
  [EventOnly]
  public event EventHandler? EventField
  {
    [MethodOnly]
    add
    {
      global::System.Console.WriteLine("This is the overridden add.");
      this.EventField_Source += value;
      this.EventField_Source += value;
    }
    [MethodOnly]
    remove
    {
      global::System.Console.WriteLine("This is the overridden remove.");
      this.EventField_Source -= value;
      this.EventField_Source -= value;
    }
  }
  private EventHandler? EventField_Source;
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Attributes_Uninlineable.EventOnlyAttribute]
  public event global::System.EventHandler? IntroducedEventField
  {
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Attributes_Uninlineable.MethodOnlyAttribute]
    add
    {
      global::System.Console.WriteLine("This is the overridden add.");
      this.IntroducedEventField_Source += value;
      this.IntroducedEventField_Source += value;
    }
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Attributes_Uninlineable.MethodOnlyAttribute]
    remove
    {
      global::System.Console.WriteLine("This is the overridden remove.");
      this.IntroducedEventField_Source -= value;
      this.IntroducedEventField_Source -= value;
    }
  }
  private global::System.EventHandler? IntroducedEventField_Source;
}