int Method(int a)
{
    int i = 0;
    while (i < 1)
    {
        i++;
    }

    global::System.Console.WriteLine("Test result = " + i);
    var result = this.Method(a);
    return (int)result;
}