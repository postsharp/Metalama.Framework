internal class TargetClass
{
    [Override]
    public int Simple()
    {
        global::System.Int32 result;
        result = Foo();
        goto __aspect_return_1;

        int Foo()
        {
            return 42;
        }
    __aspect_return_1: global::System.Console.WriteLine("This is the overriding method.");
        return (global::System.Int32)result;
    }

    [Override]
    public int Simple_Static()
    {
        global::System.Int32 result;
        result = Foo();
        goto __aspect_return_1;

        static int Foo()
        {
            return 42;
        }
    __aspect_return_1: global::System.Console.WriteLine("This is the overriding method.");
        return (global::System.Int32)result;
    }

    [Override]
    public int ParameterCapture(int x)
    {
        global::System.Int32 result;
        result = Foo();
        goto __aspect_return_1;

        int Foo()
        {
            return x + 1;
        }
    __aspect_return_1: global::System.Console.WriteLine("This is the overriding method.");
        return (global::System.Int32)result;
    }

    [Override]
    public int LocalCapture(int x)
    {
        global::System.Int32 result;
        int y = x + 1;

        result = Foo();
        goto __aspect_return_1;

        int Foo()
        {
            return y;
        }
    __aspect_return_1: global::System.Console.WriteLine("This is the overriding method.");
        return (global::System.Int32)result;
    }
}