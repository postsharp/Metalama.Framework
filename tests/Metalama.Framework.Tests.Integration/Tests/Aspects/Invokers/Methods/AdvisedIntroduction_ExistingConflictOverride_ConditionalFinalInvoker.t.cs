[TestIntroduction]
[Test]
internal class TargetClass
{
    public void VoidMethod()
    {
        this?.VoidMethod();
    }

    public int ExistingMethod()
    {
        return this?.ExistingMethod();
    }

    public int ExistingMethod_Parameterized(int x)
    {
        return this.ExistingMethod_Parameterized(x);
    }
}