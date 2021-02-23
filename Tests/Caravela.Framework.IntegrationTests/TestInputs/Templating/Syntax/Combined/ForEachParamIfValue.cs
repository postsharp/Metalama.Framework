using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.Combined.ForEachParamIfValue
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            foreach (var p in target.Parameters)
            {
                if (p.Value == null)
                {
                    throw new ArgumentNullException(p.Name);
                }
            }

            dynamic result = proceed();
            return result;
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