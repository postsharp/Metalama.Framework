using System;
using System.Threading.Tasks;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.NonAsyncTemplateOnAsyncMethod
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod(
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
        public async ValueTask<int> AsyncMethod( int a )
        {
            await Task.Yield();

            return a;
        }
    }
}