// Warning CS0414 on `EventField`: `The field 'TargetStruct.EventField' is assigned but its value is never used`
[IntroduceAspect]
public struct TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Initializers.IInterface
{
    public TargetClass()
    {
    }
    public global::System.Int32 AutoProperty { get; set; } = (global::System.Int32)42;
    public event global::System.EventHandler? EventField = default;
}