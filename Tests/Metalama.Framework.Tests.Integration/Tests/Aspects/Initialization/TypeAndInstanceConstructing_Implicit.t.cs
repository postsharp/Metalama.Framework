[Aspect]
public class TargetCode
{
    private int Method(int a)
    {
        return a;
    }


    static TargetCode()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }

    public TargetCode()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }
}