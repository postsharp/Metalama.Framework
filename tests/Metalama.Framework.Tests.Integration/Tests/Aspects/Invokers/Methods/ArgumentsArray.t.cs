internal class TargetClass
{
    [Test]
    void M(int i, int j)
    {
        this.M_Source(new global::System.Int32[] { 1, 2 }[0], new global::System.Int32[] { 1, 2 }[1]);
        return;
    }
    private void M_Source(int i, int j) => Console.WriteLine(i + j);
}
