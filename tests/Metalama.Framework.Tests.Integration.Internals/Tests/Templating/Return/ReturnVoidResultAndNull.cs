#pragma warning disable CS8600, CS8603
using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnVoidResultAndNull
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            try
            {
                dynamic result = meta.Proceed();
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