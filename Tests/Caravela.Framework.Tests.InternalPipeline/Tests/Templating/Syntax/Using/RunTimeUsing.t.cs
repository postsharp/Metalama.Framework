int Method(int a)
{
    using (new global::System.IO.MemoryStream())
    {
        var y = a + 0;
        return (int)this.Method(a);
    }

    using (global::System.IO.MemoryStream s = new global::System.IO.MemoryStream())
    {
        global::System.Console.WriteLine("");
    }
}