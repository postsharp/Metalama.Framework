internal class TargetCode
    {
        [Aspect]
        private async void MethodReturningValueTaskOfInt( int a )
        {
    global::System.Console.WriteLine("Before");
    await this.MethodReturningValueTaskOfInt_Source(a);
    object result = null;
    global::System.Console.WriteLine("After");
    return;
        }

private async global::System.Threading.Tasks.ValueTask MethodReturningValueTaskOfInt_Source(int a)
        {
            await Task.Yield();
            Console.WriteLine( "Oops" );
        }
    }