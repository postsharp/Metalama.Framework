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
    return (int)result;
}

private async Task<int> AsyncMethod_Source(int a)
        {
            await Task.Yield();
            return a;
        }
    }