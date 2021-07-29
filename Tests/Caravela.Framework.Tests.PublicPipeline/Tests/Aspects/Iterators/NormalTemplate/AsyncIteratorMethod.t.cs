using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Iterators.NormalTemplate.AsyncIteratorMethods
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

        
    }
    
    class Program
    {
        public static async Task Main()
        {
            TargetCode t = new();
            
            await foreach ( var i in t.AsyncEnumerable(0) ) 
            {
                Console.WriteLine($"  Received {i}");
            }
            
            await foreach ( var i in t.AsyncEnumerableCancellable(0, default) ) 
            {
                Console.WriteLine($"  Received {i}");
            }
            
            await using ( var enumerator = t.AsyncEnumerator(0) )
            {
                while ( await enumerator.MoveNextAsync() )
                {
                    Console.WriteLine($"  Received {enumerator.Current}");
                }
            }
            
             await using ( var enumerator = t.AsyncEnumeratorCancellable(0, default) )
            {
                while ( await enumerator.MoveNextAsync() )
                {
                    Console.WriteLine($"  Received {enumerator.Current}");
                }
            }
        }
    }

    class TargetCode
    {
        [Aspect]
        public async IAsyncEnumerable<int> AsyncEnumerable(int a)
{
    global::System.Console.WriteLine("Before AsyncEnumerable");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumerable__OriginalImpl(a)));
    global::System.Console.WriteLine("After AsyncEnumerable");
    await foreach (var r in result)
    {
        yield return r;
    }
}

private async IAsyncEnumerable<int> __AsyncEnumerable__OriginalImpl(int a)
        {
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }
        
         [Aspect]
        public async IAsyncEnumerable<int> AsyncEnumerableCancellable(int a, [EnumeratorCancellation] CancellationToken token)
{
    global::System.Console.WriteLine("Before AsyncEnumerableCancellable");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumerableCancellable__OriginalImpl(a, token), token));
    global::System.Console.WriteLine("After AsyncEnumerableCancellable");
    await foreach (var r in result)
    {
        yield return r;
    }
}

private async IAsyncEnumerable<int> __AsyncEnumerableCancellable__OriginalImpl(int a, [EnumeratorCancellation] CancellationToken token)
        {
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }
        
        
        [Aspect]
        public async IAsyncEnumerator<int> AsyncEnumerator(int a)
{
    global::System.Console.WriteLine("Before AsyncEnumerator");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumerator__OriginalImpl(a)));
    global::System.Console.WriteLine("After AsyncEnumerator");
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
}

private async IAsyncEnumerator<int> __AsyncEnumerator__OriginalImpl(int a)
        {
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }
   

         [Aspect]
        public async IAsyncEnumerator<int> AsyncEnumeratorCancellable(int a, CancellationToken token)
{
    global::System.Console.WriteLine("Before AsyncEnumeratorCancellable");
    var result = (await global::Caravela.Framework.RunTime.RunTimeAspectHelper.BufferAsync(this.__AsyncEnumeratorCancellable__OriginalImpl(a, token)));
    global::System.Console.WriteLine("After AsyncEnumeratorCancellable");
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
}

private async IAsyncEnumerator<int> __AsyncEnumeratorCancellable__OriginalImpl(int a, CancellationToken token)
        {
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }

    }
}
