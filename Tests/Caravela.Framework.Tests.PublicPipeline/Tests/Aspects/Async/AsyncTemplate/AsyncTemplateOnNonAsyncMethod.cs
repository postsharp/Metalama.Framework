using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.AsyncTemplateOnNonAsyncMethod
{
    class Aspect : Attribute, IAspect<IMethod>
    {
    
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.TargetDeclaration, 
            new( nameof(this.OverrideMethod), asyncTemplate: nameof(this.OverrideAsyncMethod), useAsyncTemplateForAnyAwaitable: true ) );
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
            var result = await meta.Proceed();
            Console.WriteLine($"result={result}");
            return result;
            
        }
    }

    // <target>
    class TargetCode
    {
        
        [Aspect]
        public ValueTask<int> AsyncMethod(int a)
        {
            return ValueTask.FromResult(a);
        }
    }
}