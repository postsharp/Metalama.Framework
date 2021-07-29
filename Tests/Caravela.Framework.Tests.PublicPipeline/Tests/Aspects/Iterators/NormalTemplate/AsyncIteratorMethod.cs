// @IgnoredDiagnostic(CS1998)

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
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Before " + meta.Method.Name);
            var result = meta.Proceed();
            Console.WriteLine("After " + meta.Method.Name);
            return result;
            
        }
        
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
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }
        
         [Aspect]
        public async IAsyncEnumerable<int> AsyncEnumerableCancellable(int a, [EnumeratorCancellation] CancellationToken token)
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
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }
   

         [Aspect]
        public async IAsyncEnumerator<int> AsyncEnumeratorCancellable(int a, CancellationToken token)
        {
            yield return 1;
            await Task.Yield();
            yield return 2;
            await Task.Yield();
            yield return 3;
        }

    }
}