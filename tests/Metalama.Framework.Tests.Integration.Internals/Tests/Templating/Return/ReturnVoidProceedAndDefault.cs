#pragma warning disable CS8600, CS8603
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnVoidProceedAndDefault
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            try
            {
                return meta.Proceed();
            }
            catch
            {
                return default;
            }
        }
    }

    class TargetCode
    {
        // <target>
        void Method(int a, int b)
        {
            Console.WriteLine(a / b);
        }
    }
}