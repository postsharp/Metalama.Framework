
[TestOutput]
[Introduction]
internal class TargetClass
{
    public int OverrideInt()
    {
        global::System.Console.WriteLine("Introduced");
        return (int)this.OverrideInt();
    }

    public void OverrideVoid()
    {
        global::System.Console.WriteLine("Introduced");
        this.OverrideVoid();
    }


    public void IntroduceVoid()
    {
        global::System.Console.WriteLine("Introduced");
    }

    public global::System.Int32 IntroduceInt()
    {
        global::System.Console.WriteLine("Introduced");
        return (int)0;
    }
}

