[TestOutput]
[Introduction]
internal class TargetClass
{
    public global::System.Int32 IntroducedProperty_Auto
    {
        get
        {
            return this.__IntroducedProperty_Auto__BackingField;
        }

        set
        {
            this.__IntroducedProperty_Auto__BackingField = value;
        }
    }

    private global::System.Int32 __IntroducedProperty_Auto__BackingField;
    public static global::System.Int32 IntroducedProperty_Auto_Static
    {
        get
        {
            return __IntroducedProperty_Auto_Static__BackingField;
        }

        set
        {
            __IntroducedProperty_Auto_Static__BackingField = value;
        }
    }

    private static global::System.Int32 __IntroducedProperty_Auto_Static__BackingField;
    public global::System.Int32 IntroducedProperty_Accessors
    {
        get
        {
            global::System.Console.WriteLine("Get");
            return (int)42;
        }

        set
        {
            global::System.Console.WriteLine(value);
        }
    }
}