// --- TypeAndInstanceConstructing_MultipleSyntaxTrees.cs ---

[Aspect]
public partial class TargetCode
{
    private int Method(int a)
    {
        return a;
    }
}

// --- TypeAndInstanceConstructing_MultipleSyntaxTrees.1.cs ---

public partial class TargetCode
{
    static TargetCode()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }
    public void Bar()
    {
    }
}

// --- TypeAndInstanceConstructing_MultipleSyntaxTrees.2.cs ---

public partial class TargetCode
{
    public TargetCode()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }
}