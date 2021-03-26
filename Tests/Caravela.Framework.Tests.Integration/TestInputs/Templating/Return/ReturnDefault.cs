#pragma warning disable CS8600, CS8603
using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnDefault
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            try
            {
                dynamic result = proceed();
                return result;
            }
            catch
            {
                return default;
            }
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return 42 / a;
        }
    }
}