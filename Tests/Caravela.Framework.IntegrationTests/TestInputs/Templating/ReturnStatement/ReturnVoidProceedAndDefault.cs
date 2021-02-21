using System;
using System.Collections.Generic;

using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.ReturnStatement.ReturnVoidProceedAndDefault
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            try
            {
                return proceed();
            }
            catch
            {
                return default;
            }
        }
    }

    class TargetCode
    {
        void Method(int a, int b)
        {
            Console.WriteLine(a / b);
        }
    }
}