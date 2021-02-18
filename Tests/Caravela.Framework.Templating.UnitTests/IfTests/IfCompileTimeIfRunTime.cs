using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.IfTests.IfCompileTimeIfRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        string Method(string a)
        {
            return a;
        }
    }
}