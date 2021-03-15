#pragma warning disable CS8600, CS8603
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.SwitchNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result;
            switch (target.Parameters.Count)
            {
                case 0:
                    result = null;
                    break;
                case 1:
                    result = target.Parameters[0].Value;
                    break;
                case 2:
                    goto default;
                case 3:
                    goto case 2;
                default:
                    result = proceed();
                    break;
            }
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}