using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfRunTimeIfCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if (target.Parameters[0].Value == null)
            {
                if (target.Method.Name == "DontThrowMethod")
                {
                    Console.WriteLine("Oops");
                }
                else
                {
                    throw new ArgumentNullException();
                }
            }

            return proceed();
        }
    }

    class TargetCode
    {
        void Method(object a)
        {
        }
    }
}