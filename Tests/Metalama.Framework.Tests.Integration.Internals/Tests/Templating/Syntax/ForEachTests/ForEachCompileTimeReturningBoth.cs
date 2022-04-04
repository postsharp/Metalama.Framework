#pragma warning disable CS0649, CS8618

using System;
using System.Collections.Generic;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEach.CompileTimeReturningBoth
{

    [CompileTime]
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
