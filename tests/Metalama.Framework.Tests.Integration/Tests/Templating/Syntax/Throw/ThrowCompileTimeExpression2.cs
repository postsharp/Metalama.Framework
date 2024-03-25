using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Throw.ThrowCompileTimeExpression2
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
             
            // Compile-time
            object? r = meta.CompileTime<object>(null);
            
            var t = r ?? throw new Exception();
        
            return null;
        }
            
    }

    class TargetCode
    {
        void Method(int a)
        {
            Console.WriteLine("Hello, world.");
        }
        
    }
}