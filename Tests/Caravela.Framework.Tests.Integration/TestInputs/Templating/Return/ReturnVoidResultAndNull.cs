#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnVoidResultAndNull
{
    [CompileTime]
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
                return null;
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