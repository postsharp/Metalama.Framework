using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Combined.ForEachParamIfValue
{
    [CompileTime]
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