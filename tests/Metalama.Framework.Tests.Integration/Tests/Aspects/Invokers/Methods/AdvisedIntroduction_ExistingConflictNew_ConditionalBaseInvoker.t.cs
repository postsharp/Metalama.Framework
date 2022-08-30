[TestIntroduction]
[Test]
internal class TargetClass
{
    public virtual void BaseClass_VoidMethod()
    {
        System.Console.WriteLine("BaseClass_VoidMethod()");
    }

    public virtual int? BaseClass_ExistingMethod()
    {
        System.Console.WriteLine("BaseClass_ExistingMethod()");
        return 42;
    }

    public virtual int? BaseClass_ExistingMethod_Parameterized(int? x)
    {
        System.Console.WriteLine("BaseClass_ExistingMethod_Parameterized()");
        return x + 42;
    }

    public void VoidMethod()
    {
        global::System.Console.WriteLine("Base method print.");
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