int Method(int a, int b)
{
    if (a <= 1)
    {
        global::System.Console.WriteLine("Oops a <= 1");
    }

    if (b <= 1)
    {
        global::System.Console.WriteLine("Oops b <= 1");
    }

    if (a <= 2)
    {
        global::System.Console.WriteLine("Oops a <= 2");
    }

    if (b <= 2)
    {
        global::System.Console.WriteLine("Oops b <= 2");
    }

    global::System.Int32 result;
    result = this.Method(a, b);
    return (int)result;
}