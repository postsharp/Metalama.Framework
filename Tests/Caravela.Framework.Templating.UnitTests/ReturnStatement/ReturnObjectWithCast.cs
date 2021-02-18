using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.ReturnStatement.ReturnObjectWithCast
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            object x = target.Parameters[0].Value;
            return x;
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}