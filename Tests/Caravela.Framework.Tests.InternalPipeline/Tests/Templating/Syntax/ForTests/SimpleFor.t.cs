int Method(int a)
{
    for (int i = 0; i < 3; i++)
    {
        try
        {
            return (int)this.Method(a);
        }
        catch
        {
        }
    }

    throw new global::System.Exception();
}