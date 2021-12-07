using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Dynamic.Issue28811
{
    
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            
            var field = meta.Target.Type.FieldsAndProperties.Single();
            
            dynamic? clone = null;
            dynamic? clonedValue = null;
            field.Invokers.Base!.SetValue(clone, clonedValue);
            
            field.Invokers.Base!.SetValue(clone, field.Invokers.Base.GetValue(meta.This));
        
            
            return default;
        }
    }
    
    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.

    // <target>
    class TargetCode
    {
        
        int a;
        
        void Method()
        {
        }
    }
}