using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.InsertStatementErrorTwoStatements
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.InsertStatement("for (;;) {} return 1;");
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