[Override]
internal partial struct TargetStruct
{
    public void TargetMethod1()
    {
        global::System.Console.WriteLine($"This is the override of TargetMethod1.");
        Console.WriteLine("This is TargetMethod1.");
        return;
    }
}

internal partial struct TargetStruct
{
    public void TargetMethod2()
    {
        global::System.Console.WriteLine($"This is the override of TargetMethod2.");
        Console.WriteLine("This is TargetMethod2.");
        return;
    }
}

internal partial struct TargetStruct
{
    public void TargetMethod3()
    {
        global::System.Console.WriteLine($"This is the override of TargetMethod3.");
        Console.WriteLine("This is TargetMethod3.");
        return;
    }
}