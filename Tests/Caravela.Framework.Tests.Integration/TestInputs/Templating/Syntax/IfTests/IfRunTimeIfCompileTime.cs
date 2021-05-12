using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfRunTimeIfCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if (meta.Parameters[0].Value == null)
            {
                if (meta.Method.Name == "DontThrowMethod")
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