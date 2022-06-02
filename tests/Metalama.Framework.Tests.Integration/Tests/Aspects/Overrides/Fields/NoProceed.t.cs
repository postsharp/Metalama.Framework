internal class TargetClass
{


    public global::System.Int32 Field
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Override.");

        }
    }

    public global::System.Int32 StaticField
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Override.");

        }
    }

    public global::System.Int32 InitializerField
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        set
        {
            global::System.Console.WriteLine("Override.");

        }
    }

    public global::System.Int32 ReadOnlyField
    {
        get
        {
            global::System.Console.WriteLine("Override.");
            return default;

        }
        private set
        {
            global::System.Console.WriteLine("Override.");

        }
    }
    public TargetClass()
    {
        this.ReadOnlyField = 42;
    }
}