internal class C
{
    [return: MyAspect]
    public int M([MyAspect] int p)
    {
        global::System.Console.WriteLine("FromTemplate: nameof(p), MyAspect, DateTime, C, UtcNow");
        global::System.Console.WriteLine("FromBuildAspect:MyAspect, DateTime, C, UtcNow");
        global::System.Int32 returnValue;
        returnValue = 0;
        goto __aspect_return_1;
    __aspect_return_1: global::System.Console.WriteLine("FromTemplate: nameof(returnValue), MyAspect, DateTime, C, UtcNow");
        global::System.Console.WriteLine("FromBuildAspect:MyAspect, DateTime, C, UtcNow");
        return returnValue;

    }
}