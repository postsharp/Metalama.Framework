class TargetCode
    {
       [Aspect1, Aspect2]
        async void MethodReturningValueTaskOfInt(int a)
{
    global::System.Console.WriteLine("Aspect1.Before");
    await this.__Override__MethodReturningValueTaskOfInt__By__Aspect2(a);
    object result_1 = null;
    global::System.Console.WriteLine("Aspect1.After");
    return;
}

private async global::System.Threading.Tasks.ValueTask __MethodReturningValueTaskOfInt__OriginalImpl(int a)
        {
            await Task.Yield();
            Console.WriteLine( "Oops" );
        }


private async global::System.Threading.Tasks.ValueTask __Override__MethodReturningValueTaskOfInt__By__Aspect2(global::System.Int32 a)
{
    global::System.Console.WriteLine("Aspect2.Before");
    await this.__MethodReturningValueTaskOfInt__OriginalImpl(a);
    object result = null;
    global::System.Console.WriteLine("Aspect2.After");
    return;
}    }
