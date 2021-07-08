int Method(int a)
{
    try
    {
        throw new global::System.ArgumentNullException("a");
    }
    catch
    {
        throw;
    }

    return (int)this.Method(a);
}