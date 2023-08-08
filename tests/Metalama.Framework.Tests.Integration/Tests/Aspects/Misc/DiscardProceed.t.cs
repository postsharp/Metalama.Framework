class Target
{
    [Test]
    MemoryStream M1()
    {
        _ = (global::System.IO.MemoryStream)(new());
        return default;
    }
    [Test]
    MemoryStream M2()
    {
        _ = (global::System.IO.MemoryStream)(new());
        return default;
    }
}
