int Method(int a, int b)
{
    global::System.Func<global::System.Int32, global::System.Int32, global::System.Int32> action = (int a_1, int b_1) => a_1 + b_1;
    global::System.Int32 result;
    result = this.Method(a, b);
    result = action(result, 2);
    return (int)result;
}