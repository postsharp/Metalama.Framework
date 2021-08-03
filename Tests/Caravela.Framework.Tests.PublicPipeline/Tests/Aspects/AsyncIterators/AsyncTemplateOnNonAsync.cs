// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System.Runtime.CompilerServices;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.AsyncIterators.AsyncTemplateOnNonAsync
{
    class Aspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.Target, 
            new( nameof(this.OverrideMethod), nameof(this.OverrideAsyncMethod), useAsyncTemplateForAnyAwaitable: true ) );
        }
    
        [Template]
        public dynamic? OverrideMethod()
        {
            throw new NotSupportedException("Should not be selected");
        }

        [Template]
        public async Task<dynamic?> OverrideAsyncMethod()
        {
            await Task.Yield();
            Console.WriteLine("Before " + meta.Target.Method.Name);
            var result = meta.Proceed();
            Console.WriteLine("After " + meta.Target.Method.Name);
            await Task.Yield();
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
       
        }
    }

    // <target>
    class TargetCode
    {
        [Aspect]
        public IAsyncEnumerable<int> AsyncEnumerable(int a)
        {
            Console.WriteLine("Not Async");
            return this.AsyncEnumerableImpl(a);
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
}