class TargetCode
    {
    
        // The normal template should be applied because YieldAwaitable does not have a method builder.
        
        [Aspect1]
        [Aspect2]
        public async Task<int> AsyncMethod(int a)
{
    global::System.Console.WriteLine("Async intercept");
    await global::System.Threading.Tasks.Task.Yield();
    var result = await this.__Override__AsyncMethod__By__Aspect2(a);
    return (int)result;
}

private async Task<int> __AsyncMethod__OriginalImpl(int a)
        {
            await Task.Yield();
            return a;
        }


public global::System.Threading.Tasks.Task<global::System.Int32> __Override__AsyncMethod__By__Aspect2(global::System.Int32 a)
{
    global::System.Console.WriteLine("Non-async intercept");
    return this.__AsyncMethod__OriginalImpl(a);
}        
        [Aspect1]
        [Aspect2]
        public  async Task<int> NonAsyncMethod(int a)
{
    global::System.Console.WriteLine("Async intercept");
    await global::System.Threading.Tasks.Task.Yield();
    var result = await this.__Override__NonAsyncMethod__By__Aspect2(a);
    return (int)result;
}


public global::System.Threading.Tasks.Task<global::System.Int32> __Override__NonAsyncMethod__By__Aspect2(global::System.Int32 a)
{
    global::System.Console.WriteLine("Non-async intercept");
            return Task.FromResult(a);
}        
    }