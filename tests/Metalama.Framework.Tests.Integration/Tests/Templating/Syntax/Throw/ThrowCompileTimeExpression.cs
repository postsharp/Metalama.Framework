using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Throw.ThrowCompileTimeExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
             
            // Compile-time
            object? r = meta.CompileTime<object>(null);
            
            // The next condition should not be reduced at build time because this would result into invalid syntax.
            var s = r != null ? 1 : throw new Exception();
        
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