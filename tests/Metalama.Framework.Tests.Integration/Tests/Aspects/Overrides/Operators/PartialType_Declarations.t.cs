[Override]
internal partial class TargetClass
{
    public static TargetClass operator +(TargetClass a, TargetClass b)
    {
        global::System.Console.WriteLine($"This is the override of op_Addition.");
        Console.WriteLine($"This is the original operator.");

        return new TargetClass();
    }
}

internal partial class TargetClass
{
    public static TargetClass operator -(TargetClass a, TargetClass b)
    {
        global::System.Console.WriteLine($"This is the override of op_Subtraction.");
        Console.WriteLine($"This is the original operator.");

        return new TargetClass();
    }
}

internal partial class TargetClass
{
    public static TargetClass operator *(TargetClass a, TargetClass b)
    {
        global::System.Console.WriteLine($"This is the override of op_Multiply.");
        Console.WriteLine($"This is the original operator.");

        return new TargetClass();
    }
}