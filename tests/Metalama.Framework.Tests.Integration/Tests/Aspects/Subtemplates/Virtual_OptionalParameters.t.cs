class TargetCode
{
    [Aspect]
    void Method1()
    {
        global::System.Console.WriteLine("regular template");
        global::System.Console.WriteLine($"called template i={1} j=2");
        global::System.Console.WriteLine($"called template i={1} j=-2");
        global::System.Console.WriteLine($"called template i={-1} j=-2");
        return;
    }
    [DerivedAspect]
    void Method2()
    {
        global::System.Console.WriteLine("regular template");
        global::System.Console.WriteLine($"derived template i={1} j=2");
        global::System.Console.WriteLine($"derived template i={1} j=-2");
        global::System.Console.WriteLine($"derived template i={-10} j=-2");
        return;
    }
}
