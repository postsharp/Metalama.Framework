int Method(int a, int b)
{
    global::System.Action<global::System.Object?> action = a_1 => global::System.Console.WriteLine(a_1_?.ToString());
    var result = this.Method(a, b);
    action(result);
    return (int)result;
}