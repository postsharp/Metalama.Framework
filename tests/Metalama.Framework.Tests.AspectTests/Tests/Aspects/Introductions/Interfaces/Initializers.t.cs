// Warning CS0414 on `EventField`: `The field 'TargetClass.EventField' is assigned but its value is never used`
[IntroduceAspect]
public class TargetClass : global::Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.Initializers.IInterface
{
  public global::System.Int32 AutoProperty { get; set; } = (global::System.Int32)42;
  public event global::System.EventHandler? EventField = (global::System.EventHandler? )default;
}