private object Method( int a, int b )
{
    unchecked
    {
    }

    checked
    {
    }

    string s = default;
    s ??= "42";
    s = s[0..2];
    global::System.Console.WriteLine(2);
    global::System.Console.WriteLine((false, true));
    global::System.Console.WriteLine(true);
    global::System.Console.WriteLine(s);
    global::System.Console.WriteLine(sizeof(bool));
    global::System.Console.WriteLine(typeof(global::System.Int32));
    var result = this.Method(a, b);
    return (global::System.Object)result;
}