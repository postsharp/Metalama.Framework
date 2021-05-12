using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lock.RunTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            lock (meta.This)
            {
                var x = meta.Parameters.Count;
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