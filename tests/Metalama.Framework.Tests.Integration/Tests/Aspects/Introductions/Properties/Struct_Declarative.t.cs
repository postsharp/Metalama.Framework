[Introduction]
internal struct TargetStruct
{
  private int _existingField;
  public TargetStruct(int x)
  {
    this._existingField = x;
  }
  public global::System.Int32 IntroducedProperty { get; set; } = default;
  public global::System.Int32 IntroducedProperty_Initializer { get; set; } = (global::System.Int32)42;
  public static global::System.Int32 IntroducedProperty_Static { get; set; } = default;
  public static global::System.Int32 IntroducedProperty_Static_Initializer { get; set; } = (global::System.Int32)42;
}