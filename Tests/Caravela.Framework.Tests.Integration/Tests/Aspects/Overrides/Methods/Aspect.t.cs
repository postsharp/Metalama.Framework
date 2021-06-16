// <target>
internal class TargetClass
{
    [Override]
    public void TargetMethod_Void()
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine("This is the original method.");
        return;
    }

    [Override]
    public void TargetMethod_Void(int x, int y)
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine($"This is the original method {x} {y}.");
        return;
    }

    [Override]
    public int TargetMethod_Int()
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine("This is the original method.");
        return 42;
    }

    [Override]
    public int TargetMethod_Int(int x, int y)
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine($"This is the original method {x} {y}.");
        return x + y;
    }

    [Override]
    public static void TargetMethod_Static()
    {
        global::System.Console.WriteLine("This is the overriding method.");
        Console.WriteLine("This is the original static method.");
        return;
    }
}