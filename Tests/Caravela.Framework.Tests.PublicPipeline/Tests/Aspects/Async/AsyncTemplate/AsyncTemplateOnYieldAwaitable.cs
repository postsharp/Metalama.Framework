using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.AsyncTemplateOnYieldAwaitable
{
    class Aspect : Attribute, IAspect<IMethod>
    {
    
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.Target, 
            new( nameof(this.OverrideMethod), 
                asyncTemplate: nameof(this.OverrideAsyncMethod), 
                useAsyncTemplateForAnyAwaitable: true ) );
        }
    
    
        [Template]
        public dynamic? OverrideMethod()
        {
            Console.WriteLine("Normal template.");
            return meta.Proceed();
        }

        [Template(TemplateKind.Async)]
        public async Task<dynamic?> OverrideAsyncMethod()
        {
            await Task.Yield();
            var result = await meta.ProceedAsync();
            Console.WriteLine($"result={result}");
            return result;
            
        }
    }

    // <target>
    class TargetCode
    {
    
        // The normal template should be applied because YieldAwaitable does not have a method builder.
        
        [Aspect]
        public System.Runtime.CompilerServices.YieldAwaitable AsyncMethod(int a)
        {
            return Task.Yield();
        }
    }
}