using System.IO;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.UsingStatement
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            using (new MemoryStream())
            {
                dynamic result = proceed();
                return result;
            }
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}