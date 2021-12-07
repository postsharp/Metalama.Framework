using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.TestApp.Aspects.Eligibility
{
    // Example of an aspect that restricts the eligibility of its parent (it cannot loosen it).
    public class MyDerivedAspect : MyAspect
    {
        public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            // Must call the base method as the first call.
            base.BuildEligibility(builder);

            builder.MustHaveAccessibility(Accessibility.Public);
        }
    }
}
