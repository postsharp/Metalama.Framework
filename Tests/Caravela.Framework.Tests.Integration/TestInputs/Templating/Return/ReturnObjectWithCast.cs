using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnObjectWithCast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            object x = target.Parameters[0].Value;
            return x;
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