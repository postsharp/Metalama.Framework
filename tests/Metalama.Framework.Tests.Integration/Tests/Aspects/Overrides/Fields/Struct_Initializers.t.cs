internal struct TargetStruct
{


    private global::System.Int32 _field = 42;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Struct_Initializers.OverrideAttribute]
    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._field;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._field = value;
        }
    }

    private static global::System.Int32 _staticField = 42;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Struct_Initializers.OverrideAttribute]
    public static global::System.Int32 StaticField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Struct_Initializers.TargetStruct._staticField;
        }
        set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Struct_Initializers.TargetStruct._staticField = value;
        }
    }
    public TargetStruct()
    {
    }
}