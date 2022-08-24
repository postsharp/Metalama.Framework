[TestIntroduction]
[Test]
internal class TargetClass
{
    public void VoidMethod_Base()
    {
        this?.VoidMethod_Base();
    }

    public int ExistingMethod_Base()
    {
        return this?.ExistingMethod_Base();
    }

    public int ExistingMethod_Parameterized_Base(int x)
    {
        return this?.ExistingMethod_Parameterized_Base(int x);
    }

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
        return this?.ExistingMethod_Parameterized(x);
    }
}