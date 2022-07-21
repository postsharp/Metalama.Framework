[Introduction]
internal class TargetClass
{
    [CopiedLog]
    public int DefaultMethod()
    {
        global::System.Console.WriteLine("TargetClass.DefaultMethod() started.");
        try
        {
            global::System.Int32 result;
            result = 0;
            goto __aspect_return_1;
            __aspect_return_1: global::System.Console.WriteLine("TargetClass.DefaultMethod() succeeded.");
            return (global::System.Int32)result;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine("TargetClass.DefaultMethod() failed: " + e.Message);
            throw;
        }
    }

    [CopiedLog]
    public global::System.Int32 IntroducedMethod_Int()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    [CopiedLog]
    public global::System.Int32 IntroducedMethod_Param(global::System.Int32 x)
    {
        global::System.Console.WriteLine($"This is introduced method, x = {x}.");
        return default(global::System.Int32);
    }

    [CopiedLog]
    public static global::System.Int32 IntroducedMethod_StaticSignature()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    [CopiedLog]
    public virtual global::System.Int32 IntroducedMethod_VirtualExplicit()
    {
        global::System.Console.WriteLine("This is introduced method.");
        return default(global::System.Int32);
    }

    [CopiedLog]
    public void IntroducedMethod_Void()
    {
        global::System.Console.WriteLine("This is introduced method.");
    }
}