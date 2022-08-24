[TestIntroduction]
[Test]
internal class TargetClass : BaseClass
{
    public void VoidMethod_Base()
    {
        System.Console.WriteLine("Base method print.");
    }

    public int ExistingMethod_Base()
    {
        return 42;
    }

    [Introduce(WhenExists = OverrideStrategy.Override)]
    public int ExistingMethod_Parameterized_Base(int x)
    {
        return x;
    }

    public void VoidMethod()
    {
        global::System.Console.WriteLine("Target method print.");
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