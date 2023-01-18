using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Pragma.InsertStatementErrorRunTimeValue
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            meta.InsertStatement(meta.This);
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