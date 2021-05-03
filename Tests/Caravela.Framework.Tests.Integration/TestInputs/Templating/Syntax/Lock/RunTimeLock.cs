using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lock.RunTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            lock (target.This)
            {
                var x = target.Parameters.Count;
                Console.WriteLine(x);
                return proceed();
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