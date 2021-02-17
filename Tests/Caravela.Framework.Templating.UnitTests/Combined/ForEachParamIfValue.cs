using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.Combined.ForEachParamIfValue
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        string Method(object a, object b)
        {
            return a.ToString() + b.ToString();
        }
    }
}