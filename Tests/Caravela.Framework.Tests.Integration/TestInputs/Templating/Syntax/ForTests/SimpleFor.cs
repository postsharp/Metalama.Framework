using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.ForTests.SimpleFor
{
    [CompileTime]
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