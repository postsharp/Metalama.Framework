internal record TargetRecord
{
    private global::System.Int32 _property;

    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Record_ImplicitProperties.OverrideAttribute]
    public global::System.Int32 Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._property;
        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            this._property = value;
        }
    }

    private static global::System.Int32 _staticProperty;

    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Record_ImplicitProperties.OverrideAttribute]
    public static global::System.Int32 StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Record_ImplicitProperties.TargetRecord._staticProperty;
        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Record_ImplicitProperties.TargetRecord._staticProperty = value;
        }
    }

    public TargetRecord(int property, int staticProperty)
    {
        Property = property;
        StaticProperty = staticProperty;
    }
}