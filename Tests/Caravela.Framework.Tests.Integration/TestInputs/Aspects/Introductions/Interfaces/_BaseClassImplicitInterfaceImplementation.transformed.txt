public class TargetClass : BaseClass, IInterface
{
    int IInterface.InterfaceMethod()
    {
        return this.InterfaceMethod();
    }

    public int InterfaceMethod()
    {
        Console.WriteLine("This is introduced interface method.");
        return base.InterfaceMethod();
    }
}