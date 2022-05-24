internal class TargetClass
{


    private int _property;
    [Override]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._property;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._property = value;
        }
    }


    private static int _staticProperty;

    [Override]
    public static int StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticProperty;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticProperty = value;
        }
    }


    private int _initializerProperty = 42;

    [Override]
    public int InitializerProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._initializerProperty;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._initializerProperty = value;
        }
    }

    private static int _staticInitializerProperty = 42;

    [Override]
    public static int StaticInitializerProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializerProperty;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
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