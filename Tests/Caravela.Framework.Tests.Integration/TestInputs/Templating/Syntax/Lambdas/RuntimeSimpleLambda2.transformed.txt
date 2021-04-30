{
    global::System.Action<global::System.Object> action = a_1 => global::System.Console.WriteLine(a_1.ToString());
    global::System.Int32 result;
    result = this.Method(a, b);
    action(result);
    return (int)result;
}
