using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfParamValue
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        string Method(object a, object b)
        {
            return a.ToString() + b.ToString();
        }
    }
}