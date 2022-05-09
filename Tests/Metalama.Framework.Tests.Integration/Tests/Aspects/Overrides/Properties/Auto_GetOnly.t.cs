[IntroduceAndOverride]
internal class TargetClass
{


    private global::System.Int32 _property;


    public global::System.Int32 Property
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

    private static global::System.Int32 _staticProperty;


    public static global::System.Int32 StaticProperty
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

    private global::System.Int32 _initializerProperty = 42;


    public global::System.Int32 InitializerProperty
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

    private static global::System.Int32 _staticInitializeProperty = 42;


    public static global::System.Int32 StaticInitializeProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializeProperty;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly.TargetClass._staticInitializeProperty = value;
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
        StaticInitializeProperty = 27;
    }


    private global::System.Int32 _introducedProperty;


    public global::System.Int32 IntroducedProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._introducedProperty;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._introducedProperty = value;
        }
    }

    private global::System.Int32 _introducedStaticProperty;


    public global::System.Int32 IntroducedStaticProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._introducedStaticProperty;
        }
        private set
        {
            global::System.Console.WriteLine($"This is the overridden setter.");
            this._introducedStaticProperty = value;
        }
    }
}