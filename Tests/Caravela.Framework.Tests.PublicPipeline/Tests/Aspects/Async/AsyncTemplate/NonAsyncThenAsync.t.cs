class TargetCode
    {
    
        [Aspect1]
        [Aspect2]
        public Task<int> AsyncMethod(int a)
{
    global::System.Console.WriteLine("Non-async intercept");
    return (System.Threading.Tasks.Task<int>)this.__Override__AsyncMethod__By__Aspect2(a);
}

private async Task<int> __AsyncMethod__OriginalImpl(int a)
        {
            await Task.Yield();
            return a;
        }


public async global::System.Threading.Tasks.Task<global::System.Int32> __Override__AsyncMethod__By__Aspect2(global::System.Int32 a)
{
    global::System.Console.WriteLine("Async intercept");
    await global::System.Threading.Tasks.Task.Yield();
    var result = await this.__AsyncMethod__OriginalImpl(a);
    return (int)result;
}        
        [Aspect1]
        [Aspect2]
        public Task<int> NonAsyncMethod(int a)
{
    global::System.Console.WriteLine("Non-async intercept");
    return (System.Threading.Tasks.Task<int>)this.__Override__NonAsyncMethod__By__Aspect2(a);
}

private Task<int> __NonAsyncMethod__OriginalImpl(int a)
        {
            return Task.FromResult(a);
        }


public async global::System.Threading.Tasks.Task<global::System.Int32> __Override__NonAsyncMethod__By__Aspect2(global::System.Int32 a)
{
    global::System.Console.WriteLine("Async intercept");
    await global::System.Threading.Tasks.Task.Yield();
    var result = await this.__NonAsyncMethod__OriginalImpl(a);
    return (int)result;
}        
    }
