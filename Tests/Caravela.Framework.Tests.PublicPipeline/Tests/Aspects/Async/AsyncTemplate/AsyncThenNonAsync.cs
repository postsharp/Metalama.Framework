using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.AsyncThenNonAsync
{
    class Aspect1 : Attribute, IAspect<IMethod>
    {
    
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.TargetDeclaration, 
            new( nameof(this.OverrideMethod), 
                asyncTemplate: nameof(this.OverrideAsyncMethod), 
                useAsyncTemplateForAnyAwaitable: true ) );
        }
    
    
        [Template]
        public dynamic? OverrideMethod()
        {
            throw new NotSupportedException("This should not be called.");
        }

        [Template]
        public async Task<dynamic?> OverrideAsyncMethod()
        {
            Console.WriteLine("Async intercept");
            await Task.Yield();
            var result = await meta.Proceed();
            return result;
            
        }
    }
    
    class Aspect2 : Attribute, IAspect<IMethod>
    {
    
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( builder.TargetDeclaration, 
            new( nameof(this.OverrideMethod), 
                asyncTemplate: nameof(this.OverrideAsyncMethod), 
                useAsyncTemplateForAnyAwaitable: true ) );
        }
    
    
        [Template]
        public dynamic? OverrideMethod()
        {
            throw new NotSupportedException("This should not be called.");
        }

        [Template]
        public Task<dynamic?> OverrideAsyncMethod()
        {
            Console.WriteLine("Non-async intercept");
            return meta.Proceed();
            
        }
    }

    // <target>
    class TargetCode
    {
    
        // The normal template should be applied because YieldAwaitable does not have a method builder.
        
        [Aspect1]
        [Aspect2]
        public async Task<int> AsyncMethod(int a)
        {
            await Task.Yield();
            return a;
        }
        
        [Aspect1]
        [Aspect2]
        public Task<int> NonAsyncMethod(int a)
        {
            return Task.FromResult(a);
        }
        
    }
}