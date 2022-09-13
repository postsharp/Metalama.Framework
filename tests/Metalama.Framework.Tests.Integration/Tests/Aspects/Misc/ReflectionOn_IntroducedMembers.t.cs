[Introduction]
internal class Target
{

    public global::System.Int32 IntroducedField;

    public global::System.Int32 IntroducedField_Initializer = (global::System.Int32)42;

    private global::System.Int32 IntroducedField_Initializer_Private = (global::System.Int32)42;

    private global::System.Int32 IntroducedField_Private;

    public static global::System.Int32 IntroducedField_Static;

    public static global::System.Int32 IntroducedField_Static_Initializer = (global::System.Int32)42;

    private static global::System.Int32 IntroducedField_Static_Private;

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

    private global::System.Int32 IntroducedProperty_Accessors_Private
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

    private global::System.Int32 IntroducedProperty_Auto_GetOnly_Initializer_Private { get; } = (global::System.Int32)42;

    private global::System.Int32 IntroducedProperty_Auto_GetOnly_Private { get; }

    public global::System.Int32 IntroducedProperty_Auto_Initializer { get; set; } = (global::System.Int32)42;

    private global::System.Int32 IntroducedProperty_Auto_Initializer_Private { get; set; } = (global::System.Int32)42;

    private global::System.Int32 IntroducedProperty_Auto_Private { get; set; }

    public static global::System.Int32 IntroducedProperty_Auto_Static { get; set; }

    private static global::System.Int32 IntroducedProperty_Auto_Static_Private { get; set; }

    public T GenericMethod<T>(T a)
    {
        return (T)a;
    }

    public global::System.Int32 IntroducedMethod_Int()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    private global::System.Int32 IntroducedMethod_Int_Private()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    public global::System.Int32 IntroducedMethod_Param(global::System.Int32 x)
    {
        global::System.Console.WriteLine($"This is introduced method, x = {x}.");
        return default(global::System.Int32);
    }

    private global::System.Int32 IntroducedMethod_Param_Private(global::System.Int32 x)
    {
        global::System.Console.WriteLine($"This is introduced method, x = {x}.");
        return default(global::System.Int32);
    }

    public static global::System.Int32 IntroducedMethod_StaticSignature()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    private static global::System.Int32 IntroducedMethod_StaticSignature_Private()
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
        global::System.Console.WriteLine(this.IntroducedField_Initializer_Private);
    }

    private void IntroducedMethod_Void_Private()
    {
        global::System.Console.WriteLine("This is introduced method.");
    }

    public void OutMethod(out global::System.Int32 x)
    {
        x = 42;
        global::System.Console.WriteLine("OutMethod with parameter.");
    }

    public global::System.Int32 RefMethod(ref global::System.Int32 x)
    {
        x += 42;
        return (global::System.Int32)42;
    }

    public event global::System.EventHandler? EventField;
}