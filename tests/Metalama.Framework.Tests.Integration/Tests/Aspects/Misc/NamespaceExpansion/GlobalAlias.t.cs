internal class TargetClass
{
    [Override]
    public void TargetMethod_Void()
    {
        global::System.Console.WriteLine(global::System.Math.PI);
        Console.WriteLine("This is the original method.");
        return;
    }
}