#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnVoidProceedAndDefault
{
    [CompileTime]
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