using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.SimpleAssignments
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var n = meta.Target.Parameters.Count; // build-time
                                             //var n = meta.RunTime(target.Method.Parameters.Count); // run-time
            var a0 = meta.Target.Parameters[0].Value; // run-time
            var x = 0; // run-time
            var y = meta.CompileTime(0); // compile-time    

            Console.WriteLine(n);
            Console.WriteLine(a0);
            Console.WriteLine(x);
            Console.WriteLine(y);

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