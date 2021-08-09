class TargetCode
    {
        
        [Aspect]
        public  async ValueTask<int> AsyncMethod(int a)
{
    await global::System.Threading.Tasks.Task.Yield();
    var result = await this.AsyncMethod_Source(a);
    global::System.Console.WriteLine($"result={result}");
    return (global::System.Int32)(result);
}

private ValueTask<int> AsyncMethod_Source(int a)
        {
            return ValueTask.FromResult(a);
        }
    }