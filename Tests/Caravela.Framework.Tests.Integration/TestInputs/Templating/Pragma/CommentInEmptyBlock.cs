using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Pragma.CommentInEmptyBlock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if ( false )
            {
                pragma.Comment("Oops 1");
            }
            return proceed();
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