[Introduction]
[Override]
internal class TargetClass : BaseClass
{
    public void VoidMethod()
    {
        global::System.Console.WriteLine("Override");
        this.VoidMethod();
        return;
    }

    public int ExistingMethod()
    {
        global::System.Console.WriteLine("Override");
        return this.ExistingMethod();
    }

    public int ExistingMethod_Parameterized(int x)
    {
        global::System.Console.WriteLine("Override");
        return this.ExistingMethod_Parameterized(x);
    }


    public override global::System.Int32 BaseClass_ExistingMethod()
    {
        // Introduced.
        return this.BaseClass_ExistingMethod();
    }

    public override global::System.Int32 BaseClass_ExistingMethod_Parameterized(global::System.Int32 x)
    {
        // Introduced.
        return this.BaseClass_ExistingMethod_Parameterized(x);
    }

    public override void BaseClass_VoidMethod()
    {
        // Introduced.
        this.BaseClass_VoidMethod();
    }
}