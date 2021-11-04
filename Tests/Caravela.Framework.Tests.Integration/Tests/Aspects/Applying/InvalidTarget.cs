using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.IntegrationTests.Aspects.Applying.InvalidTarget
{
    // Intentionally not using TypeAspect so we have no AttributeUsage.
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder ) { }

        public void BuildAspectClass( IAspectClassBuilder builder ) { }

        public void BuildEligibility( IEligibilityBuilder<INamedType> builder ) { }
    }

    // <target>
    internal class TargetClass
    {
        [Introduction]
        private void Method() { }
    }
}