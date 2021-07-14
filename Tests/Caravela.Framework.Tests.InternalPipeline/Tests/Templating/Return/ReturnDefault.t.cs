int Method(int a)
{
    try
    {
        var result = this.Method(a);
        return (int)result;
    }
    catch
    {
        return default;
    }
}