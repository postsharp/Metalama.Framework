class TargetCode
{
    [Aspect]
    async void MethodReturningValueTaskOfInt(int a)
    {
        global::System.Console.WriteLine("Before");
        await this.__MethodReturningValueTaskOfInt__OriginalImpl(a);
        object result = null;
        global::System.Console.WriteLine("After");
        return;
    }

    private async global::System.Threading.Tasks.ValueTask __MethodReturningValueTaskOfInt__OriginalImpl(int a)
    {
        await Task.Yield();
        Console.WriteLine( "Oops" );
    }
}