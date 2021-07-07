int Method(out int a)
{
    global::System.Int32 result;
    result = this.Method(out a);
    a = 5;
    return (int)result;
}