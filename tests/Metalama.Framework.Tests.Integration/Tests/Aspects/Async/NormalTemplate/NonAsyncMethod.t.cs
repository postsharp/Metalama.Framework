internal class TargetCode
{
    [Aspect]
    public Task<int> TaskReturningNonAsync(int a)
    {
        global::System.Console.WriteLine("Before");
        global::System.Threading.Tasks.Task<global::System.Int32> result;
        result = Task.FromResult(a);
        goto __aspect_return_1;
    __aspect_return_1: global::System.Console.WriteLine("After");
        return (global::System.Threading.Tasks.Task<global::System.Int32>)result;
    }

    [Aspect]
    public ValueTask<int> ValueTaskReturningNonAsync(int a)
    {
        global::System.Console.WriteLine("Before");
        global::System.Threading.Tasks.ValueTask<global::System.Int32> result;
        result = new ValueTask<int>(0);
        goto __aspect_return_1;
    __aspect_return_1: global::System.Console.WriteLine("After");
        return (global::System.Threading.Tasks.ValueTask<global::System.Int32>)result;
    }
}