#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER) - The exception message string is slightly different in .NET Framework.
#endif

using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.InsertStatementErrorMissingToken
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            meta.InsertStatement( "return 1" );

            return default;
        }
    }

    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}