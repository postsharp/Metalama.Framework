// <target>
internal class TargetClass
{
    [Override]
    public void TargetMethod_Void_TwoReturns(int x)
    {
        global::System.Console.WriteLine("Override.");
        while (x > 0)
        {
            if (x == 42)
            {
                goto __aspect_return_1;
            }

            x--;
        }

        if (x > 0)
            goto __aspect_return_1;
    __aspect_return_1:
        ;
        return;
    }

    [Override]
    public int TargetMethod_Int_TwoReturns(int x)
    {
        global::System.Console.WriteLine("Override.");
        while (x > 0)
        {
            if (x == 42)
            {
                return 42;
            }

            x--;
        }

        if (x > 0)
            return -1;

        return 0;
    }
}