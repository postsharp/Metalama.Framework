int Method(int a)
{
    int i = 0;
    while (true)
    {
        i++;
        if (i >= 1)
            break;
    }
    global::System.Console.WriteLine("Test result = " + i);
    return this.Method(a);
}
