internal class TargetClass
{
    private int _field;

    [FirstOverride]
    [SecondOverride]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.Property_Source;
            return this.Property_Source;


        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.Property_Source = value;

        }
    }

    private int Property_Source
    {
        get
        {
            return _field;
        }

        set
        {
            _field = value;
        }
    }

    private static int _staticField;

    [FirstOverride]
    [SecondOverride]
    public static int StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates.TargetClass.StaticProperty_Source;
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates.TargetClass.StaticProperty_Source;


        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates.TargetClass.StaticProperty_Source = value;

        }
    }

    private static int StaticProperty_Source
    {
        get
        {
            return _staticField;
        }

        set
        {
            _staticField = value;
        }
    }

    [FirstOverride]
    [SecondOverride]
    public int ExpressionBodiedProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.ExpressionBodiedProperty_Source;
            return this.ExpressionBodiedProperty_Source;


        }
    }

    private int ExpressionBodiedProperty_Source => 42


            [FirstOverride]
            [SecondOverride]
        public int AutoProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.AutoProperty_Source;
            return this.AutoProperty_Source;



        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.AutoProperty_Source = value;


        }
    }

    private int AutoProperty_Source { get; set; }

    //[FirstOverride]
    //[SecondOverride]
    //public int GetOnlyAutoProperty { get; }

    [FirstOverride]
    [SecondOverride]
    public int InitializerAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.InitializerAutoProperty_Source;
            return this.InitializerAutoProperty_Source;



        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.InitializerAutoProperty_Source = value;


        }
    }
    private int InitializerAutoProperty_Source { get; set; } = 42

        public TargetClass()
    {
        // this.GetOnlyAutoProperty = 42;
    }
}