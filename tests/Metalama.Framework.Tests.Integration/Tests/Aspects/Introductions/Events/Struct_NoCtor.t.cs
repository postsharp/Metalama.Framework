// Warning CS0414 on `IntroducedEvent`: `The field 'TargetStruct.IntroducedEvent' is assigned but its value is never used`
// Warning CS0414 on `IntroducedEvent_Static`: `The field 'TargetStruct.IntroducedEvent_Static' is assigned but its value is never used`
[Introduction]
internal struct TargetStruct
{
  public int ExistingField;
  public int ExistingProperty { get; set; }
  public event global::System.EventHandler? IntroducedEvent = default;
  public event global::System.EventHandler? IntroducedEvent_Static = default;
}