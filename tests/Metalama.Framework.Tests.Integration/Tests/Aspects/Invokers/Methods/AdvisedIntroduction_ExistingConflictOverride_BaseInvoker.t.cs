[Introduction]
    [Override]
    internal class TargetClass : BaseClass
    {
        public void VoidMethod()
        {
            global::System.Console.WriteLine("Override");
    this.VoidMethod_Source();
    return;
        }

private void VoidMethod_Source()
        {
            System.Console.WriteLine("TargetClass_VoidMethod()");
        }

        public int ExistingMethod()
        {
            global::System.Console.WriteLine("Override");
    return this.ExistingMethod_Source();
        }

private int ExistingMethod_Source()
        {
            System.Console.WriteLine("TargetClass_ExistingMethod()");
            return 42;
        }

        public int ExistingMethod_Parameterized(int x)
        {
            global::System.Console.WriteLine("Override");
    return this.ExistingMethod_Parameterized_Source(x);
        }

private int ExistingMethod_Parameterized_Source(int x)
        {
            System.Console.WriteLine("TargetClass_ExistingMethod_Parameterized");
            return x + 42;
        }


public override void BaseClass_VoidMethod()
{
    // Introduced.
    base.BaseClass_VoidMethod();
}

public override global::System.Int32 BaseClass_ExistingMethod()
{
    // Introduced.
    return base.BaseClass_ExistingMethod();
}

public override global::System.Int32 BaseClass_ExistingMethod_Parameterized(global::System.Int32 x)
{
    // Introduced.
    return base.BaseClass_ExistingMethod_Parameterized(x);
}    }