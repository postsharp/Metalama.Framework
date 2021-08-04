class TargetCode
    {
        
        [Aspect]
        public  async ValueTask<int> AsyncMethod(int a)
{
    await global::System.Threading.Tasks.Task.Yield();
    var result = await this.__AsyncMethod__OriginalImpl(a);
    global::System.Console.WriteLine($"result={result}");
    return (int)result;
}

private ValueTask<int> __AsyncMethod__OriginalImpl(int a)
        {
            return ValueTask.FromResult(a);
        }
    }
