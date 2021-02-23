using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfCompileTimeIfRunTime
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            var p = target.Parameters[0];
            if (target.Method.Name == "NotNullMethod")
            {
                if (p.Value == null)
                {
                    throw new ArgumentNullException(p.Name);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(p.Value))
                {
                    throw new ArgumentException("IsNullOrEmpty", p.Name);
                }
            }

            return proceed();
        }
    }

    internal class TargetCode
    {
        private string Method(string a)
        {
            return a;
        }
    }
}