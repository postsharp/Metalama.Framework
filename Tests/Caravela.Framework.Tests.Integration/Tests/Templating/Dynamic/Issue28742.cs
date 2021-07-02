using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

#pragma warning disable CS0169, CS8618

namespace Caravela.Framework.Tests.Integration.Tests.Templating.Dynamic.Issue28742
{
    
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            foreach ( var fieldOrProperty in meta.Type.FieldsAndProperties )
            {
                if ( fieldOrProperty.IsAutoPropertyOrField )
                {
                    var value = fieldOrProperty.Invokers.Final.GetValue(fieldOrProperty.IsStatic ? null : meta.This);
                    Console.WriteLine($"{fieldOrProperty.Name}={value}");
                }
            }
            
            return default;
        }
    }
    
    // Placeholder implementation of a cache because the hosted try.postsharp.net does not allow for MemoryCache.

    // <target>
    class TargetCode
    {
        
        int a;
        public string B { get; set; }

        static int c;
        
        void Method()
        {
        }
    }
}