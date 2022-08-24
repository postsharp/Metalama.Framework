[TestIntroduction]
[Test]
internal class TargetClass
{
    public new int ExistingMethod()
    {
        // Introduced.
        this.Print();
        System.Console.WriteLine("Introduced method print.");
    }
    public new int ExistingMethod()
    {
        // Introduced.
        this.Print();
        return (global::System.Int32)100;
    }

    public new int ExistingMethod_Parameterized(int x)
    {
        // Introduced.
        this.Print();
        return (global::System.Int32)(x + 100);
    }

    public void Print()
    {
        global::System.Console.WriteLine("Print() called.");
    }
}