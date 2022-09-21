internal class C
{
    [return: MyAspect]
    public int M([MyAspect] int p)
    {
        global::System.Console.WriteLine("FromTemplate: nameof(p), MyAspect, DateTime, C, UtcNow");
        global::System.Console.WriteLine("FromBuildAspect:MyAspect, DateTime, C, UtcNow");
        global::System.Int32 returnValue;
        returnValue = 0;
        global::System.Console.WriteLine("FromTemplate: nameof(returnValue), MyAspect, DateTime, C, UtcNow");
        global::System.Console.WriteLine("FromBuildAspect:MyAspect, DateTime, C, UtcNow");
        return returnValue;

    }
}