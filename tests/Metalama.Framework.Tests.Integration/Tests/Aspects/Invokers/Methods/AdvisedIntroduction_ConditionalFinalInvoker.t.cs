[TestIntroduction]
[Test]
internal class TargetClass
{

    public global::System.Int32? Method(global::System.Int32? x)
    {
        return this?.Method(x);
    }

    public void VoidMethod()
    {
        this?.VoidMethod();
        return;
    }
}