internal class TargetClass
{
    [Test]
    void M(int i, int j)
    {
        this.M_Source(1, 2);
        this.M_Source((global::System.Int32)new object[] { 1, 2 }[0], (global::System.Int32)new object[] { 1, 2 }[1]);
        return;
    }
    private void M_Source(int i, int j) => Console.WriteLine(i + j);
    [Test]
    void M(int i, params int[] a)
    {
        this.M_Source(1, 2);
        this.M_Source((global::System.Int32)new object[] { 1, 2 }[0], global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Cast<global::System.Int32>(global::System.Linq.Enumerable.Skip(new object[] { 1, 2 }, 1))));
        return;
    }
    private void M_Source(int i, params int[] a) => Console.WriteLine(a[0] + a[1]);
    [Test]
    void M(params int[] a)
    {
        this.M_Source(1, 2);
        this.M_Source(global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Cast<global::System.Int32>(new object[] { 1, 2 })));
        return;
    }
    private void M_Source(params int[] a) => Console.WriteLine(a[0] + a[1]);
}
