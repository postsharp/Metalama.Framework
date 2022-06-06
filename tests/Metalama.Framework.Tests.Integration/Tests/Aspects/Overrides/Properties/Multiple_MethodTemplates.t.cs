[IntroduceAndOverride]
internal class TargetClass
{


    private global::System.Int32 _field
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this._field_Source;
            return this._field_Source;



        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this._field_Source = value;


        }
    }
    private global::System.Int32 _field_Source { get; set; }

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


    private static global::System.Int32 _staticField
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates.TargetClass._staticField_Source;
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates.TargetClass._staticField_Source;



        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates.TargetClass._staticField_Source = value;


        }
    }
    private static global::System.Int32 _staticField_Source { get; set; }

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

    public int GetOnlyAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.GetOnlyAutoProperty_Source;
            return this.GetOnlyAutoProperty_Source;



        }
        private set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.GetOnlyAutoProperty_Source = value;


        }
    }

    private int GetOnlyAutoProperty_Source { get; set; }

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
        this.GetOnlyAutoProperty = 42;
    }


    public global::System.Int32 IntroducedAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.IntroducedAutoProperty_Source;
            return this.IntroducedAutoProperty_Source;



        }
        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.IntroducedAutoProperty_Source = value;


        }
    }
    private global::System.Int32 IntroducedAutoProperty_Source { get; set; }


    public global::System.Int32 IntroducedGetOnlyAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.IntroducedGetOnlyAutoProperty_Source;
            return this.IntroducedGetOnlyAutoProperty_Source;



        }
        private set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.IntroducedGetOnlyAutoProperty_Source = value;


        }
    }
    private global::System.Int32 IntroducedGetOnlyAutoProperty_Source { get; set; }


    public global::System.Int32 IntroducedProperty_IntroduceAndOverride
    {
        get
        {
            return (global::System.Int32)42;

        }

        set
        {
        }
    }

    public global::System.Int32 IntroducedProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            global::System.Console.WriteLine("This is the overridden getter.");
            _ = this.IntroducedProperty_IntroduceAndOverride;
            return this.IntroducedProperty_IntroduceAndOverride;


        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            global::System.Console.WriteLine("This is the overridden setter.");
            this.IntroducedProperty_IntroduceAndOverride = value;

        }
    }
}