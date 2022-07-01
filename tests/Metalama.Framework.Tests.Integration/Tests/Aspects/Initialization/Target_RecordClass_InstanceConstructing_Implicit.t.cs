[Aspect]
public record class TargetRecordClass
{
    private int Method(int a)
    {
        return a;
    }


    public TargetRecordClass()
    {
        global::System.Console.WriteLine($"TargetRecordClass: Aspect");
    }
}