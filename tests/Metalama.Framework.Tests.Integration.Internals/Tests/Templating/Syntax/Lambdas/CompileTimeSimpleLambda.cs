using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.CompileTimeSimpleLambda
{
    class Aspect
    {     
        [TestTemplate]
        dynamic Template()
        {
            Func<int, int> action = meta.CompileTime(new  Func<int, int> (x => x + 1));
        
            var result = meta.CompileTime(1);

            result = action(result);

            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}