{
    global::System.Func<global::System.Int32, global::System.Int32> action = (int x) => x + 1;
    global::System.Int32 result;
    result = this.Method(a, b);
    action(result);
    return (int)result;
}
