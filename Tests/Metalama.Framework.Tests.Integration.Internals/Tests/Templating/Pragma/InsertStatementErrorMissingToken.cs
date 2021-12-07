// @RequiredConstant(NET5_0_OR_GREATER) - The exception message string is slightly different in .NET Framework.

using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.InsertStatementErrorMissingToken
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.InsertStatement("return 1");
            return default;
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