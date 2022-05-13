internal class TargetClass
{
    private int _field;

    [FirstOverride]
    [SecondOverride]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return _field;


        }

        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
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
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return _staticField;


        }

        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            _staticField = value;

        }
    }

    [FirstOverride]
    [SecondOverride]
    public int ExpressionBodiedProperty
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return 42;

        }
    }


    private int _autoProperty;


    [FirstOverride]
    [SecondOverride]
    public int AutoProperty
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return this._autoProperty;


        }
        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            this._autoProperty = value;

        }
    }


    private int _initializerAutoProperty = 42;

    //[FirstOverride]
    //[SecondOverride]
    //public int GetOnlyAutoProperty { get; }

    [FirstOverride]
    [SecondOverride]
    public int InitializerAutoProperty
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return this._initializerAutoProperty;


        }
        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            this._initializerAutoProperty = value;

        }
    }
    public TargetClass()
    {
        //this.GetOnlyAutoProperty = 42;
    }
}