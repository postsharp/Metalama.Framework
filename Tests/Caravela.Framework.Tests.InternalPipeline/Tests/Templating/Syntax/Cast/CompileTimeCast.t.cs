int Method(int a)
{
    global::System.Console.WriteLine("a = " + a);
    return (int)this.Method(a);
}