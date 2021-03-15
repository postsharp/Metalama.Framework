using System.IO;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.UsingStatement
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            using (new MemoryStream())
            {
                dynamic result = proceed();
                return result;
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