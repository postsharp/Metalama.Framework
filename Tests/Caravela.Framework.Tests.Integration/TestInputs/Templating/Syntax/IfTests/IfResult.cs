#pragma warning disable CS8600, CS8603
using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfResult
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();

            if (result == null)
            {
                return "";
            }

            return result;
        }
    }

    class TargetCode
    {
        string Method(object a)
        {
            return a?.ToString();
        }
    }
}