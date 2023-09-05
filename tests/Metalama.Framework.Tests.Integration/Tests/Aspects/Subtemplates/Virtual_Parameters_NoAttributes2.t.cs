class TargetCode
{
    [Aspect]
    void Method1()
    {
        global::System.Console.WriteLine("regular template");
        global::System.Console.WriteLine($"called template i={1} j=2");
        return;
    }
    [DerivedAspect]
    void Method2()
    {
        global::System.Console.WriteLine("regular template");
        global::System.Console.WriteLine($"derived template i={1} j=2");
        return;
    }
    [DerivedAspect]
    Task Method3()
    {
        global::System.Console.WriteLine($"derived template i={3} j=4");
        return this.Method3_Source();
    }
    private async Task Method3_Source()
    {
        await Task.Yield();
    }
}
