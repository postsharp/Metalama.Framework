[Aspect]
public class TargetCode
{
    public TargetCode()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }
    static TargetCode()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }
    private int Method(int a)
    {
        return a;
    }
}