{
    try
    {
        return (int)1;
    }
    catch (global::System.Exception e) when (e.GetType().Name.Contains("DivideByZero"))
    {
        return (int)-1;
    }
}
