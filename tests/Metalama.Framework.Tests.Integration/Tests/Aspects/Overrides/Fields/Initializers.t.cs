[Introduction]
internal class TargetClass
{
    private global::System.Int32 _field = 42;
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field.OverrideAttribute]
    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._field;
        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            this._field = value;
        }
    }
    private static global::System.Int32 _staticField = 42;
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field.OverrideAttribute]
    public static global::System.Int32 StaticField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field.TargetClass._staticField;
        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field.TargetClass._staticField = value;
        }
    }
    static TargetClass()
    {
        StaticField = 27;
    }
    public TargetClass()
    {
        Field = 27;
    }
    public global::System.Int32 IntroducedField = (global::System.Int32)global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field.TargetClass.StaticField;
    public static global::System.Int32 IntroducedStaticField = (global::System.Int32)global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field.TargetClass.StaticField;
}
