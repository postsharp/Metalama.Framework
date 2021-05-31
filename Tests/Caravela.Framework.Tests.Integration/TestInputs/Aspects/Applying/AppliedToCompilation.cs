using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

[assembly: Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation.MyAspect]

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.AppliedToCompilation
{
    public class MyAspect : Attribute, IAspect<ICompilation>
    {
        public void BuildAspect(IAspectBuilder<ICompilation> builder) { }

        public void BuildAspectClass( IAspectClassBuilder builder ) { }

        public void BuildEligibility(IEligibilityBuilder<ICompilation> builder) { }
    }

    [TestOutput]
    internal class TargetClass
    {
    }
}
