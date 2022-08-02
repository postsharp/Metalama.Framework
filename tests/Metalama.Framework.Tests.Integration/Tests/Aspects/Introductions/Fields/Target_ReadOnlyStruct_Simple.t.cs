[Introduction]
internal readonly struct TargetStruct
{
    private readonly int _existingField;

    public TargetStruct(int existingField)
: this()
    {
        this._existingField = existingField;
    }


    public readonly global::System.Int32 IntroducedField = default;

    public readonly global::System.Int32 IntroducedField_Initializer = (global::System.Int32)42;

    public static readonly global::System.Int32 IntroducedField_Static = default;

    public static readonly global::System.Int32 IntroducedField_Static_Initializer = (global::System.Int32)42;
}