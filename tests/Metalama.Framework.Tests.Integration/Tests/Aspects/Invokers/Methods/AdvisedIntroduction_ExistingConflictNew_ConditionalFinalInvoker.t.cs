[TestIntroduction]
[Test]
internal class TargetClass
{
    public int VoidMethod()
    {
        // Introduced.
        this.VoidMethod();
        return;
    }

    public int ExistingMethod()
    {
        // Introduced.
        return this.ExistingMethod();
    }

    public int ExistingMethod_Parameterized(int x)
    {
        // Introduced.
        return this.ExistingMethod_Parameterized(int x);
    }
}