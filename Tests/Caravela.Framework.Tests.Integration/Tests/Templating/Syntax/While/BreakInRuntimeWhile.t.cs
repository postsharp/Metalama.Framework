int Method(int a)
{
    int i = 0;
    while (i < 1)
    {
        i++;
        break;
    }

    global::System.Console.WriteLine("Test result = " + i);
    global::System.Int32 result;
    result = this.Method(a);
    return (int)result;
}