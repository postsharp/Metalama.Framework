[Introduction]
    internal class TargetClass
    {
        public int OverrideInt() 
{
    global::System.Console.WriteLine("Introduced");
    return this.__OverrideInt__OriginalImpl();
}

private int __OverrideInt__OriginalImpl()
        {
            return 1;
        }
        
        public void OverrideVoid()
{
    global::System.Console.WriteLine("Introduced");
    this.__OverrideVoid__OriginalImpl();
}

private void __OverrideVoid__OriginalImpl()
        {
        }


public void IntroduceVoid()
{
    global::System.Console.WriteLine("Introduced");
}

public global::System.Int32 IntroduceInt()
{
    global::System.Console.WriteLine("Introduced");
    return 0;
}        
    }