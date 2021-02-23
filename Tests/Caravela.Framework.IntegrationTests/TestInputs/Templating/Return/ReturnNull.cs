#pragma warning disable CS8600, CS8603
using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.ReturnStatement.ReturnNull
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var a = target.Parameters[0];
            var b = target.Parameters[1];
            if (a.Value == null || b.Value == null)
            {
                return null;
            }
            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        string Method(string a, string b)
        {
            return a + b;
        }
    }
}