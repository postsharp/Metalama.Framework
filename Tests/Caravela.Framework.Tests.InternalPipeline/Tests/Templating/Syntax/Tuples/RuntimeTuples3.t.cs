int Method(int a)
{
    (int, string) anonymT = (4, "");
    global::System.Console.WriteLine(anonymT.Item1);
    return (int)this.Method(a);
}