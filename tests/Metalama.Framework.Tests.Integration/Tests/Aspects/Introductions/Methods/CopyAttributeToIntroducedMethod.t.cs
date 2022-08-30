[Introduction]
internal class TargetClass
{
    [Override]
    public int DefaultMethod()
    {
        global::System.Console.WriteLine("Start.");
        try
        {
            global::System.Int32 result;
            Console.WriteLine("This is original method.");

            result = 0;
            goto __aspect_return_1;
        __aspect_return_1: global::System.Console.WriteLine("Try.");
            return (global::System.Int32)result;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine(" Catch: " + e.Message);
            throw;
        }
    }

    [Override]
    public global::System.Int32 IntroducedMethod_Int()
    {
        global::System.Console.WriteLine("Start.");
        try
        {
            global::System.Int32 result;
            global::Console.WriteLine("This is introduced method.");

            result = 42;
            goto __aspect_return_1;
        __aspect_return_1: global::System.Console.WriteLine("Try.");
            return (global::System.Int32)result;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine(" Catch: " + e.Message);
            throw;
        }
    }

    [Override]
    public global::System.Int32 IntroducedMethod_Param(global::System.Int32 x)
    {
        global::System.Console.WriteLine("Start.");
        try
        {
            global::System.Int32 result;
            global::System.Console.WriteLine($"This is introduced method, x = {x}.");

            result = x;
            goto __aspect_return_1;
        __aspect_return_1: global::System.Console.WriteLine("Try.");
            return (global::System.Int32)result;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine(" Catch: " + e.Message);
            throw;
        }
    }

    [Override]
    public static global::System.Int32 IntroducedMethod_StaticSignature()
    {
        global::System.Console.WriteLine("Start.");
        try
        {
            global::System.Int32 result;
            global::System.Console.WriteLine("This is introduced method.");

            result = 42;
            goto __aspect_return_1;
        __aspect_return_1: global::System.Console.WriteLine("Try.");
            return (global::System.Int32)result;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine(" Catch: " + e.Message);
            throw;
        }
    }

    [Override]
    public virtual global::System.Int32 IntroducedMethod_VirtualExplicit()
    {
        global::System.Console.WriteLine("Start.");
        try
        {
            global::System.Int32 result;
            global::System.Console.WriteLine("This is introduced method.");

            result = 42;
            goto __aspect_return_1;
        __aspect_return_1: global::System.Console.WriteLine("Try.");
            return (global::System.Int32)result;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine(" Catch: " + e.Message);
            throw;
        }
    }

    [Override]
    public void IntroducedMethod_Void()
    {
        global::System.Console.WriteLine("Start.");
        try
        {
            global::System.Console.WriteLine("This is introduced method.");
            global::System.Console.WriteLine("Try.");

            return;
        }
        catch (global::System.Exception e)
        {
            global::System.Console.WriteLine(" Catch: " + e.Message);
            throw;
        }
    }
}