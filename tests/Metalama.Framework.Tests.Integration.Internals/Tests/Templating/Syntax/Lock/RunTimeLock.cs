using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lock.RunTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            lock (meta.This)
            {
                var x = meta.Target.Parameters.Count;
                Console.WriteLine(x);
                return meta.Proceed();
            }
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