internal class TargetClass
{
    [Test]
    public void VoidMethod()
    {
        this.VoidMethod_Source();
        return;
    }

    private void VoidMethod_Source()
    {
    }

    [Test]
    public int Method(int x)
    {
        return this.Method_Source(x);
    }

    private int Method_Source(int x)
    {
        return x;
    }
}