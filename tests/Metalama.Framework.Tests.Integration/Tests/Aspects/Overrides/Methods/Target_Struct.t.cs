internal struct TargetStruct
{
    [Override]
    public void TargetMethod_Void()
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine("This is the original method.");
        return;
    }

    [Override]
    public int TargetMethod_Int()
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine("This is the original method.");
        return 42;
    }
}