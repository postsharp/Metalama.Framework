// Warning CS0414 on `IntroducedEvent`: `The field 'TargetStruct.IntroducedEvent' is assigned but its value is never used`
// Warning CS0414 on `IntroducedEvent_Static`: `The field 'TargetStruct.IntroducedEvent_Static' is assigned but its value is never used`
[Introduction]
internal struct TargetStruct
{
  public TargetStruct()
  {
  }
  public int ExistingField = 42;
  public int ExistingProperty { get; set; } = 42;
  public event global::System.EventHandler? IntroducedEvent = default;
  public event global::System.EventHandler? IntroducedEvent_Static = default;
}