// <target>
internal class TargetClass
{
    [Override]
    public void TargetMethod_Void(int x)
    {
        global::System.Console.WriteLine("Begin override.");
        global::Caravela.Framework.Aspects.__Void result;
        Console.WriteLine("Begin target.");

        if (x == 0)
            goto __aspect_return_1;
        Console.WriteLine("End target.");
    __aspect_return_1:
        ;
        global::System.Console.WriteLine("End override.");
        return;
    }

    [Override]
    public int TargetMethod_Int(int x)
    {
        global::System.Console.WriteLine("Begin override.");
        global::System.Int32 result;
        Console.WriteLine("Begin target.");

        if (x == 0)
        {
            result = 42;
            goto __aspect_return_1;
        }

        Console.WriteLine("End target.");
        result = x;
        goto __aspect_return_1;
    __aspect_return_1:
        ;
        global::System.Console.WriteLine("End override.");
        return (int)result;
    }
}