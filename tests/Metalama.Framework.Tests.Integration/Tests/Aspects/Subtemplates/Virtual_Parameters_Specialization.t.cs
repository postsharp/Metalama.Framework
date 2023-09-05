class TargetCode
{
    [Aspect]
    void Method1()
    {
        global::System.Console.WriteLine("regular template");
        global::System.Console.WriteLine("called template x=TargetCode.Method1()");
        global::System.Console.WriteLine("called template x=TargetCode.Method1()");
        global::System.Console.WriteLine("called template x=TargetCode.Method1()");
        global::System.Console.WriteLine("derived template x=TargetCode.Method1()");
        return;
    }
}
