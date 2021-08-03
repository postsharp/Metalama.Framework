// Warning CS1998 on `__AsyncMethod__OriginalImpl`: `This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.`
class TargetCode
    {
        
        [Aspect]
        public ValueTask<int> AsyncMethod(int a)
{
    global::System.Console.WriteLine("Getting task");
    var task = this.__AsyncMethod__OriginalImpl(a);
    global::System.Console.WriteLine("Got task");
    return (System.Threading.Tasks.ValueTask<int>)task;
}

private async ValueTask<int> __AsyncMethod__OriginalImpl(int a)
        {
            return a;
        }
    }
