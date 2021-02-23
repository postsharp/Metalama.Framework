using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfParamValue
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            if (target.Parameters[0].Value == null)
            {
                throw new ArgumentNullException(target.Parameters[0].Name);
            }

            var p = target.Parameters[1];
            if (p.Value == null)
            {
                throw new ArgumentNullException(p.Name);
            }

            return proceed();
        }
    }

    internal class TargetCode
    {
        private string Method(object a, object b)
        {
            return a.ToString() + b.ToString();
        }
    }
}