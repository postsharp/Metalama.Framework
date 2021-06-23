internal class TargetClass
{
    [Override]
    public void TargetMethod_OutParam(out int a)
    {
        global::System.Console.WriteLine("This is the overriding method.");
        a = 0;
        return;
    }

    [Override]
    public void TargetMethod_RefParam(ref int a)
    {
        global::System.Console.WriteLine("This is the overriding method.");
        a = 0;
        return;
    }

    [Override]
    public void TargetMethod_InParam(in DateTime a)
    {
        global::System.Console.WriteLine("This is the overriding method.");
        return;
    }
}