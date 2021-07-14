int Method(int a)
{
    lock (this)
    {
        global::System.Console.WriteLine(1);
        return (int)this.Method(a);
    }
}