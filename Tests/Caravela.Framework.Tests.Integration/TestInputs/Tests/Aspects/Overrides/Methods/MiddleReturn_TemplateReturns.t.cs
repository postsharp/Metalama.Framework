// <target>
internal class TargetClass
{
    [Override]
    public void TargetMethod_Void(int x)
    {
        global::System.Console.WriteLine("Override.");
        Console.WriteLine("Begin target.");

        if (x == 0)
        {
            goto __aspect_return_1;
        }

        Console.WriteLine("End target.");
    __aspect_return_1:
        ;
        return;
    }

    [Override]
    public int TargetMethod_Int(int x)
    {
        global::System.Console.WriteLine("Override.");
        Console.WriteLine("Begin target.");

        if (x == 0)
        {
            return 42;
        }

        Console.WriteLine("End target.");

        return x;
    }
}