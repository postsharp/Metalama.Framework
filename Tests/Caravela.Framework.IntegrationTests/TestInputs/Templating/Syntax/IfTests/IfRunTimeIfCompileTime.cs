using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfRunTimeIfCompileTime
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private void Method(object a)
        {
        }
    }
}