using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

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
