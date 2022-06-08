internal class TargetClass
{
    [Test]
    public void VoidMethod()
    {
        this.VoidMethod();
        return;
    }

    [Test]
    public int Method(int x)
    {
        return this.Method(x);
    }
}