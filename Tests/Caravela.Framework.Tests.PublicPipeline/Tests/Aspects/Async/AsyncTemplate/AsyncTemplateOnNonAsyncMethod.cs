using System;
using System.Threading.Tasks;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.AsyncTemplateOnNonAsyncMethod
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod(
                builder.Target,
                new MethodTemplateSelector( nameof(OverrideMethod), asyncTemplate: nameof(OverrideAsyncMethod), useAsyncTemplateForAnyAwaitable: true ) );
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            return meta.Proceed();
        }

        [Template]
        public async Task<dynamic?> OverrideAsyncMethod()
        {
            await Task.Yield();
            var result = await meta.ProceedAsync();
            Console.WriteLine( $"result={result}" );

            return result;
        }
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        public ValueTask<int> AsyncMethod( int a )
        {
            return new ValueTask<int>( Task.FromResult( a ) );
        }
    }
}