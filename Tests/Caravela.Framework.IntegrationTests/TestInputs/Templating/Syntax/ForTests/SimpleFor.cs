using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForTests.SimpleFor
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}