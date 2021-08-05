class TargetCode
    {
        [Aspect]
        public async IAsyncEnumerable<int> AsyncEnumerable(int a)
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

private async IAsyncEnumerable<int> __AsyncEnumerable__OriginalImpl(int a)
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
        
         [Aspect]
        public async IAsyncEnumerable<int> AsyncEnumerableCancellable(int a, [EnumeratorCancellation] CancellationToken token)
{
    await global::System.Threading.Tasks.Task.Yield();
    global::System.Console.WriteLine("Before AsyncEnumerableCancellable");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumerableCancellable__OriginalImpl(a, token), token));
    global::System.Console.WriteLine("After AsyncEnumerableCancellable");
    await global::System.Threading.Tasks.Task.Yield();
    await foreach (var r in result)
    {
        yield return r;
    }

    yield break;
}

private async IAsyncEnumerable<int> __AsyncEnumerableCancellable__OriginalImpl(int a, [EnumeratorCancellation] CancellationToken token)
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
        
        
        [Aspect]
        public async IAsyncEnumerator<int> AsyncEnumerator(int a)
{
    await global::System.Threading.Tasks.Task.Yield();
    global::System.Console.WriteLine("Before AsyncEnumerator");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumerator__OriginalImpl(a)));
    global::System.Console.WriteLine("After AsyncEnumerator");
    await global::System.Threading.Tasks.Task.Yield();
    var enumerator = result;
    try
    {
        while (await enumerator.MoveNextAsync())
        {
            yield return enumerator.Current;
        }
    }
    finally
    {
        await enumerator.DisposeAsync();
    }

    yield break;
}

private async IAsyncEnumerator<int> __AsyncEnumerator__OriginalImpl(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            await Task.Yield();
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            await Task.Yield();
            yield return 3;
        }
   

         [Aspect]
        public async IAsyncEnumerator<int> AsyncEnumeratorCancellable(int a, CancellationToken token)
{
    await global::System.Threading.Tasks.Task.Yield();
    global::System.Console.WriteLine("Before AsyncEnumeratorCancellable");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumeratorCancellable__OriginalImpl(a, token)));
    global::System.Console.WriteLine("After AsyncEnumeratorCancellable");
    await global::System.Threading.Tasks.Task.Yield();
    var enumerator = result;
    try
    {
        while (await enumerator.MoveNextAsync())
        {
            yield return enumerator.Current;
        }
    }
    finally
    {
        await enumerator.DisposeAsync();
    }

    yield break;
}

private async IAsyncEnumerator<int> __AsyncEnumeratorCancellable__OriginalImpl(int a, CancellationToken token)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            await Task.Yield();
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            await Task.Yield();
            yield return 3;
        }

    }