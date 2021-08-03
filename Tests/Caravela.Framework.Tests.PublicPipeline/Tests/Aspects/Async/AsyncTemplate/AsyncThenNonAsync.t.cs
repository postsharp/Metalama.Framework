class TargetCode
    {
    
        // The normal template should be applied because YieldAwaitable does not have a method builder.
        
        [Aspect1]
        [Aspect2]
        public Task<int> AsyncMethod(int a)
{
    global::System.Console.WriteLine("Non-async intercept");
    return (System.Threading.Tasks.Task<int>)this.__Override__AsyncMethod__By__Aspect1(a);
}

private async Task<int> __AsyncMethod__OriginalImpl(int a)
        {
            await Task.Yield();
            return a;
        }


public async global::System.Threading.Tasks.Task<global::System.Int32> __Override__AsyncMethod__By__Aspect1(global::System.Int32 a)
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
    throw new global::System.NotSupportedException("This should not be called.");
}
        
    }
