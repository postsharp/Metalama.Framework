class TargetCode
{
    [Aspect]
    int NormalMethod(int a)
    {
        return a;
    }

    [Aspect]
    async Task<int> AsyncMethod(int a)
    {
        await global::System.Threading.Tasks.Task.Yield();
        var result = await this.AsyncMethod_Source(a);
        global::System.Console.WriteLine($"result={result}");
        return (global::System.Int32)result;
    }

    private async Task<int> AsyncMethod_Source(int a)
    {
        await Task.Yield();
        return a;
    }

    [Aspect]
    async Task AsyncVoidMethod()
    {
        await global::System.Threading.Tasks.Task.Yield();
        await this.AsyncVoidMethod_Source();
        object result = null;
        global::System.Console.WriteLine($"result={result}");
        return;
    }

    private async Task AsyncVoidMethod_Source()
    {
        await Task.Yield();
    }
}