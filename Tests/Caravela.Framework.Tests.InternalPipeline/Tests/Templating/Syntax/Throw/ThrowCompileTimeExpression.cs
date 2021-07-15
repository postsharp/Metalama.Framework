using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Throw.ThrowCompileTimeExpression
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