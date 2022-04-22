int Method(int a)
{
    try
    {
        var result = await this.Method(a);
        return (global::System.Int32)result;
    }
    catch
    {
        return default;
    }
}