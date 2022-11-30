using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

#pragma warning disable CS8632 // Cannot convert null literal to non-nullable reference type.

namespace Metalama.Framework.Tests.Integration.TestInputs.MagicKeywords.GenericCompileTimeWithRunTimeTypeArg
{
    namespace UsingStatic
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                var x = meta.CompileTime<TargetCode?>(null);
                var y = meta.CompileTime<TargetCode>(null);

                return meta.Proceed();
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
}