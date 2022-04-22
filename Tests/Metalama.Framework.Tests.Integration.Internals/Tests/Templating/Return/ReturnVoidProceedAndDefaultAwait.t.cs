void Method(int a, int b)
{
    try
    {
        await this.Method(a, b);
        return;
    }
    catch
    {
        return;
    }
}