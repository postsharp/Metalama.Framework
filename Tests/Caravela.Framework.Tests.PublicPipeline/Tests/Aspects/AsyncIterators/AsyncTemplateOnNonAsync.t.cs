class TargetCode
    {
        [Aspect]
public IAsyncEnumerable<int> AsyncEnumerable(int a)
{
    return this.__Override__AsyncEnumerable__By__Aspect(a);
}

private IAsyncEnumerable<int> __AsyncEnumerable__OriginalImpl(int a)
        {
            Console.WriteLine("Not Async");
            return this.AsyncEnumerableImpl(a);
        }


public async global::System.Collections.Generic.IAsyncEnumerable<global::System.Int32> __Override__AsyncEnumerable__By__Aspect(global::System.Int32 a)
{
    await global::System.Threading.Tasks.Task.Yield();
    global::System.Console.WriteLine("Before AsyncEnumerable");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumerable__OriginalImpl(a)));
    global::System.Console.WriteLine("After AsyncEnumerable");
    await global::System.Threading.Tasks.Task.Yield();
    await foreach (var r in result)
    {
        yield return r;
    }

    yield break;
}        
        private async IAsyncEnumerable<int> AsyncEnumerableImpl(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            await Task.Yield();
            Console.WriteLine("Yield 2");
            yield return 2;
            await Task.Yield();
            Console.WriteLine("Yield 3");
            yield return 3;
        }
    }
