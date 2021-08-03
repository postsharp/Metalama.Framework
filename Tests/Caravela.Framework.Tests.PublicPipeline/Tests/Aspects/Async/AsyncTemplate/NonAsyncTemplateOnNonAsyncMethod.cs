using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Async.AsyncTemplate.NonAsyncTemplateOnNonAsyncMethod
{
    class Aspect : Attribute, IAspect<IMethod>
    {
    
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.AdviceFactory.OverrideMethod( 
            builder.Target, 
            new( nameof(this.OverrideMethod), asyncTemplate: nameof(this.OverrideAsyncMethod),
             useAsyncTemplateForAnyAwaitable: true) );
        }
    
    
        [Template]
        public dynamic? OverrideMethod()
        {
            Console.WriteLine("Should not be selected.");
            return meta.Proceed();
        }

        [Template]
        public Task<dynamic?> OverrideAsyncMethod()
        {
            Console.WriteLine("Getting task");
            var task = meta.Proceed()!;
            Console.WriteLine("Got task");
            return task;
            
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