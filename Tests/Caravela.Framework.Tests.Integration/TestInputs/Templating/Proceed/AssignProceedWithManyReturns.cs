using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Proceed.AssignProceedWithManyReturns
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        bool Method(int a)
        {
            if (a % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}