[TestIntroduction]
[Test]
internal class TargetClass : BaseClass
{
    public void VoidMethod()
    {
        // Introduced.
        global::System.Console.WriteLine("Introduced method print.");
    }
    public int ExistingMethod()
    {
        // Introduced.
        return (global::System.Int32)100;
    }

    public int ExistingMethod_Parameterized(int x)
    {
        // Introduced.
        return (global::System.Int32)(x + 100);
    }

    public override global::System.Int32 ExistingMethod_Base()
    {
        // Introduced.
        return (global::System.Int32)100;
    }

    public override global::System.Int32 ExistingMethod_Parameterized_Base(global::System.Int32 x)
    {
        // Introduced.
        return (global::System.Int32)(x + 100);
    }

    public override void VoidMethod_Base()
    {
        // Introduced.
        global::System.Console.WriteLine("Introduced method print.");
    }
}