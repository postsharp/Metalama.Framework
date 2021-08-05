using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfRunTimeIfCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            if (meta.Target.Parameters[0].Value == null)
            {
                if (meta.Target.Method.Name == "DontThrowMethod")
                {
                    Console.WriteLine("Oops");
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }

            return meta.Proceed();
        }
    }

    class TargetCode
    {
        void Method(object a)
        {
        }
    }
}