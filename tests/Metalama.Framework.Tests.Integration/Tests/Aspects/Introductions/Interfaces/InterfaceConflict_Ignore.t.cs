[Introduction]
public class TargetClass : IInterface
{
    int IInterface.InterfaceMethod()
    {
        Console.WriteLine("This is the original implementation.");
        return 42;
    }
    public global::System.Int32 InterfaceMethod()
    {
        global::System.Console.WriteLine("This is introduced interface method.");
        return default(global::System.Int32);
    }
}