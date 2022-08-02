internal class TargetClass
{
    private int _field;

    [Override]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            _ = this.Property_Source;
            return this.Property_Source;
        }

        set
        {
            global::System.Console.WriteLine("Override.");
            this.Property_Source = value;
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

    [Override]
    public static int StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            _ = global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable.TargetClass.StaticProperty_Source;
            return global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable.TargetClass.StaticProperty_Source;
        }

        set
        {
            global::System.Console.WriteLine("Override.");
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable.TargetClass.StaticProperty_Source = value;
            global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable.TargetClass.StaticProperty_Source = value;
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

    [Override]
    public int ExpressionBodiedProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            _ = this.ExpressionBodiedProperty_Source;
            return this.ExpressionBodiedProperty_Source;

        }
    }

    private int ExpressionBodiedProperty_Source
    => 42;
    [Override]
    public int AutoProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            _ = this.AutoProperty_Source;
            return this.AutoProperty_Source;

        }
        set
        {
            global::System.Console.WriteLine("Override.");
            this.AutoProperty_Source = value;
            this.AutoProperty_Source = value;

        }
    }

    private int AutoProperty_Source
    { get; set; }
    [Override]
    public int AutoGetOnlyProperty
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            _ = this.AutoGetOnlyProperty_Source;
            return this.AutoGetOnlyProperty_Source;

        }
        private init
        {
            global::System.Console.WriteLine("Override.");
            this.AutoGetOnlyProperty_Source = value;
            this.AutoGetOnlyProperty_Source = value;

        }
    }

    private int AutoGetOnlyProperty_Source
    { get; set; }
}