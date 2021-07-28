#pragma warning disable CS0649, CS8618

using System;
using System.Collections.Generic;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.ForEach.CompileTimeReturningBoth
{

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public IEnumerable<int> compileTimeEnumerable = new[] { 1, 2, 3 };
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var compileTimeObject = new CompileTimeClass();

            foreach (var x in compileTimeObject.compileTimeEnumerable)
            {
                Console.WriteLine( x.ToString() );
            }
            
            foreach (var x in meta.RunTime( compileTimeObject.compileTimeEnumerable) )
            {
                Console.WriteLine( x.ToString() );
            }


            return meta.Proceed();
        }
    }
    
    class TargetCode
    {
        void Method() {}
    }
}
