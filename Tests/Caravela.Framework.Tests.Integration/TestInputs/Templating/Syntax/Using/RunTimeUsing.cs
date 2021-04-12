using System.IO;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Using.RunTimeUsing
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            using (new MemoryStream())
            {
                return proceed();
            }
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