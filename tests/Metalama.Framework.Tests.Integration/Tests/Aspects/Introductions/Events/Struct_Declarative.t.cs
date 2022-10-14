// Warning CS0414 on `IntroducedEvent`: `The field 'TargetStruct.IntroducedEvent' is assigned but its value is never used`
// Warning CS0414 on `IntroducedEvent_Static`: `The field 'TargetStruct.IntroducedEvent_Static' is assigned but its value is never used`
[Introduction]
internal struct TargetStruct
{
  private int _existingField;
  public TargetStruct(int x)
  {
    this._existingField = x;
  }
  public event global::System.EventHandler? IntroducedEvent = default;
  public event global::System.EventHandler? IntroducedEvent_Static = default;
}