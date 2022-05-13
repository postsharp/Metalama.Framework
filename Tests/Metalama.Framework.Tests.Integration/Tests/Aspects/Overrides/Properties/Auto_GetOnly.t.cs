internal class TargetClass
{


    private global::System.Int32 _property;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.OverrideAttribute]
    public global::System.Int32 Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._property;
        }
        private set
        {
            this._property = value;
        }
    }

    private static global::System.Int32 _staticProperty;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.OverrideAttribute]
    public static global::System.Int32 StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticProperty;
        }
        private set
        {
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticProperty = value;
        }
    }

    private global::System.Int32 _initializerProperty = 42;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.OverrideAttribute]
    public global::System.Int32 InitializerProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._initializerProperty;
        }
        private set
        {
            this._initializerProperty = value;
        }
    }

    private static global::System.Int32 _staticInitializerProperty = 42;


    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.OverrideAttribute]
    public static global::System.Int32 StaticInitializerProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializerProperty;
        }
        private set
        {
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializerProperty = value;
        }
    }
    public TargetClass()
    {
        this.Property = 42;
        this.InitializerProperty = 27;
    }

    static TargetClass()
    {
        StaticProperty = 42;
        StaticInitializerProperty = 27;
    }
}