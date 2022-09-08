[Introduction]
internal class Target
{
    public global::System.Int32 IntroducedField;

    public global::System.Int32 IntroducedField_Initializer = (global::System.Int32)42;

    public static global::System.Int32 IntroducedField_Static;

    public static global::System.Int32 IntroducedField_Static_Initializer = (global::System.Int32)42;

    public global::System.Int32 IntroducedProperty_Accessors
    {
        get
        {
            global::System.Console.WriteLine("Get");
            return (global::System.Int32)42;
        }

        set
        {
            global::System.Console.WriteLine(value);
        }
    }

    public global::System.Int32 IntroducedProperty_Auto { get; set; }

    public global::System.Int32 IntroducedProperty_Auto_GetOnly { get; }

    public global::System.Int32 IntroducedProperty_Auto_GetOnly_Initializer { get; } = (global::System.Int32)42;

    public global::System.Int32 IntroducedProperty_Auto_Initializer { get; set; } = (global::System.Int32)42;

    public static global::System.Int32 IntroducedProperty_Auto_Static { get; set; }

    public T GenericMethod<T>(T a)
    {
        return (T)a;
    }

    public global::System.Int32 IntroducedMethod_Int()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    public global::System.Int32 IntroducedMethod_Param(global::System.Int32 x)
    {
        global::System.Console.WriteLine($"This is introduced method, x = {x}.");
        return default(global::System.Int32);
    }

    public static global::System.Int32 IntroducedMethod_StaticSignature()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    public virtual global::System.Int32 IntroducedMethod_VirtualExplicit()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    public void IntroducedMethod_Void()
    {
        global::System.Console.WriteLine("This is introduced method.");
    }

    public event global::System.EventHandler? EventField;
}