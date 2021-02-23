using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForTests.SimpleFor
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return proceed();
                }
                catch
                {
                }
            }

            throw new Exception();
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}