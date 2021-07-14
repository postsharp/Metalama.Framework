int Method(int a, int bb)
{
    global::System.Console.WriteLine(1);
    var result = this.Method(a, bb);
    return (int)result;
}