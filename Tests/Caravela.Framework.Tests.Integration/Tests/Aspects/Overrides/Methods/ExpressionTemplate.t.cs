public class Target
    {
        [Test]
        public int Foo()
{
    return default;
}

    [Test]
    public void Bar()
    {
        _ = default;
        return;
    }
}