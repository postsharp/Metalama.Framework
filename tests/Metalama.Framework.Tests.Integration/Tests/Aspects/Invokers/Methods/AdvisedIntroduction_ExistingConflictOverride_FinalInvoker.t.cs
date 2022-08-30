[TestIntroduction]
[Test]
internal class TargetClass : BaseClass
{
    public void VoidMethod()
    {
        this.VoidMethod();
        return;
    }

    public int ExistingMethod()
    {
        return this.ExistingMethod();
    }

    public int ExistingMethod_Parameterized(int x)
    {
        return this.ExistingMethod_Parameterized(x);
    }

    public override global::System.Int32 BaseClass_ExistingMethod()
    {
        return this.BaseClass_ExistingMethod();
    }

    public override global::System.Int32 BaseClass_ExistingMethod_Parameterized(global::System.Int32 x)
    {
        return this.BaseClass_ExistingMethod_Parameterized(x);
    }

    public override void BaseClass_VoidMethod()
    {
        this.BaseClass_VoidMethod();
        return;
    }

    public void Print()
    {
        this.Print();
        return;
    }
}