using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

[assembly: Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation.MyAspect]

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation
{
    public class MyAspect : Attribute, IAspect<ICompilation>
    {
        public void Initialize(IAspectBuilder<ICompilation> aspectBuilder)
        {
        }
    }

    [TestOutput]
    internal class TargetClass
    {
    }
}
