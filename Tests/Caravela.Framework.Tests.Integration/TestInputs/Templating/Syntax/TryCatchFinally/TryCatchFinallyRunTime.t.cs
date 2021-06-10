{
    try
    {
        global::System.Console.WriteLine("try");
        global::System.Int32 result;
        result = this.Method(a);
        global::System.Console.WriteLine("success");
        return (int)result;
    }
    catch
    {
        global::System.Console.WriteLine("exception 0");
        throw;
    }
    finally
    {
        global::System.Console.WriteLine("finally");
    }

    global::System.Console.WriteLine(0);
}
