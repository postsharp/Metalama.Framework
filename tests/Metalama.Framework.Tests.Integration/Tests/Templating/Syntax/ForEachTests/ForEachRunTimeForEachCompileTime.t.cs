int Method(int a, int b)
{
    global::System.Collections.Generic.IEnumerable<global::System.Int32> array = global::System.Linq.Enumerable.Range(1, 2);
    foreach (int n in array)
    {
        if (a <= n)
        {
            global::System.Console.WriteLine("Oops a <= " + n);
        }
        if (b <= n)
        {
            global::System.Console.WriteLine("Oops b <= " + n);
        }
    }
    var result = this.Method(a, b);
    return (global::System.Int32)result;
}
