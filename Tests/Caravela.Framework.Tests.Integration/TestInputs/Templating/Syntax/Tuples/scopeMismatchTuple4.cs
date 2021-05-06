using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.ScopeMismatchTuples4
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var a = 1;
            var b = 2; 
            
            var namedItems = meta.CompileTime((a, b));
            Console.WriteLine(namedItems.a);
            
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}