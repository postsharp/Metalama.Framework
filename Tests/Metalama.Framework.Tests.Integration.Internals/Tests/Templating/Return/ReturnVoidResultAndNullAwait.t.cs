void Method(int a, int b)
{
    try
    {
        await this.Method(a, b);
        object result = null;
        return;
    }
    catch
    {
        return;
    }
}
