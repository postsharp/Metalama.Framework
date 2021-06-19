[TestOutput]
[Introduction]
internal class TargetClass
{
    public global::System.Int32 AutoProperty
    {
        get
        {
            return this.__AutoProperty__BackingField;
        }

        set
        {
            this.__AutoProperty__BackingField = value;
        }
    }

    private global::System.Int32 __AutoProperty__BackingField;
    public global::System.Int32 Property
    {
        get
        {
            global::System.Console.WriteLine("Get");
            return default(global::System.Int32);
        }

        set
        {
            global::System.Console.WriteLine("Set");
            global::System.Int32 discard;
        }
    }

    public global::System.Int32 PropertyFromAccessors
    {
        get
        {
            global::System.Console.WriteLine("Get");
            return default(global::System.Int32);
        }

        set
        {
            global::System.Console.WriteLine("Set");
            global::System.Int32 discard;
        }
    }
}