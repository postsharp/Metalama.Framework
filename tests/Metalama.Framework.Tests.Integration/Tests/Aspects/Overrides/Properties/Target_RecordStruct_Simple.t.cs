internal record struct TargetRecordStruct
{
    private int _field;

    [Override]
    public int Property
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return this._field;
        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            this._field = value;
        }
    }

    private static int _staticField;

    [Override]
    public static int StaticProperty
    {
        get
        {
            global::System.Console.WriteLine("This is the overridden getter.");
            return _staticField;
        }

        set
        {
            global::System.Console.WriteLine("This is the overridden setter.");
            _staticField = value;
        }
    }
}