internal class TargetClass
{
    private global::System.Int32 _field;

    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return this._field;

        }
        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            this._field = value;

        }
    }

    private static global::System.Int32 _staticField;

    public global::System.Int32 StaticField
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return this._staticField;

        }
        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            this._staticField = value;

        }
    }

    private global::System.Int32 _initializerField = 42;

    public global::System.Int32 InitializerField
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return this._initializerField;

        }
        set
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            this._initializerField = value;

        }
    }

    private readonly global::System.Int32 _readOnlyField;

    public global::System.Int32 ReadOnlyField
    {
        get
        {
            global::System.Console.WriteLine("First override.");
            global::System.Console.WriteLine("Second override.");
            return this._readOnlyField;

        }
    }
    public TargetClass()
    {
        this._readOnlyField = 42;
    }
}