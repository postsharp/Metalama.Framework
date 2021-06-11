{
    var x = new { A = a, B = b, Count = 2 };
    var y = new { Count = 2 };
    global::System.Console.WriteLine(x);
    global::System.Console.WriteLine(x.A);
    global::System.Console.WriteLine(x.Count);
    global::System.Console.WriteLine(y.Count);
    global::System.Int32 result;
    result = this.Method(a, b);
    return (int)result;
}