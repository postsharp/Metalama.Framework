internal class TargetClass
{
    [Override]
    public void VoidMethod(int x)
    {
        global::System.Console.WriteLine("Override.");
        Console.WriteLine("Begin target.");

        if (x == 0)
        {
            goto __aspect_return_1;

        }

        Console.WriteLine("End target.");
    __aspect_return_1: return;
    }

    [Override]
    public int Method(int x)
    {
        global::System.Console.WriteLine("Override.");
        Console.WriteLine("Begin target.");

        if (x == 0)
        {
            _ = 42;
            goto __aspect_return_1;
        }

        Console.WriteLine("End target.");

        _ = x;
        goto __aspect_return_1;
    __aspect_return_1: return default;
    }

    [Override]
    public T? GenericMethod<T>(T? x)
    {
        global::System.Console.WriteLine("Override.");
        Console.WriteLine("Begin target.");

        if (x?.Equals(default) ?? false)
        {
            _ = x;
            goto __aspect_return_1;
        }

        Console.WriteLine("End target.");

        _ = x;
        goto __aspect_return_1;
    __aspect_return_1: return default;
    }
}