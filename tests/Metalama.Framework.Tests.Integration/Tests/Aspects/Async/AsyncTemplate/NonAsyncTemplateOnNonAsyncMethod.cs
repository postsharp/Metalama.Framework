using System;
using System.Threading.Tasks;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.NonAsyncTemplateOnNonAsyncMethod
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advice.Override(
                builder.Target,
                new MethodTemplateSelector(
                    nameof(OverrideMethod),
                    asyncTemplate: nameof(OverrideAsyncMethod),
                    useAsyncTemplateForAnyAwaitable: true ) );
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            Console.WriteLine( "Should not be selected." );

            return meta.Proceed();
        }

        [Template]
        public Task<dynamic?> OverrideAsyncMethod()
        {
            Console.WriteLine( "Getting task" );
            var task = meta.ProceedAsync()!;
            Console.WriteLine( "Got task" );

            return task;
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