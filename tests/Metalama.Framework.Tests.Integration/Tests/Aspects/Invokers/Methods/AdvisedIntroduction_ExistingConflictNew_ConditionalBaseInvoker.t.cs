[TestIntroduction]
[Test]
internal class TargetClass
{
    public void VoidMethod()
    {
        return this?.VoidMethod();
    }

    public int ExistingMethod()
    {
        return (global::System.Int32)42;
    }

    public int ExistingMethod_Parameterized(int x)
    {
        return (global::System.Int32)(x);
    }
}