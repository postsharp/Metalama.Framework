{
    global::System.Func<global::System.Int32, global::System.Int32> action = x => x + 1;
    global::System.Int32 result;
    result = this.Method(a, b);
    action(result);
    return (int)result;
}
