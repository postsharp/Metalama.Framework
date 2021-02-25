using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfRunTimeIfCompileTime
{
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