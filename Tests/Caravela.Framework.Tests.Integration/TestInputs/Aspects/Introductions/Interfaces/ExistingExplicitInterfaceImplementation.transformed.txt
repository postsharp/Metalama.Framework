public class TargetClass : ISuperInterface
{
    int IInterface.ISubInterface()
    {
        return this.SubInterfaceMethod();
    }
            
    public int SubInterfaceMethod()
    {
        Console.WriteLine("This is introduced interface method.");
        Console.WriteLine("This is original interface method.");
        return 27;
    }

    int IInterface.ISuperInterface()
    {
        return this.SuperInterfaceMethod();
    }
            
    public int SuperInterfaceMethod()
    {
        Console.WriteLine("This is introduced interface method.");
        return default;
    }
}