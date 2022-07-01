[Aspect]
public record struct TargetRecordStruct
{
    private int Method(int a)
    {
        return a;
    }


    public TargetRecordStruct()
    {
        global::System.Console.WriteLine($"TargetRecordStruct: Aspect");
    }
}