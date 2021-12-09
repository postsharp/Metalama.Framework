using System;
using System.Collections.Generic;
using System.Text;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.TestApp.Aspects.Eligibility
{
    // Example of an aspect with complex eligibility rules.
    public class MyAspect : Attribute, IAspect<IMethod>
    {
        public virtual void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            builder.MustBeNonStatic();

            builder
                .DeclaringType()
                .MustSatisfyAll(
                    and =>
                    {
                        and.MustHaveAccessibility(Accessibility.Public);
                        and.MustBeNonAbstract();
                    });

            builder.ReturnType().MustBe(typeof(void));
            builder.ExceptForScenarios(EligibleScenarios.Inheritance).MustBeNonAbstract();

            builder.Parameter(0)
                .Type()
                .MustSatisfyAny(
                    or =>
                    {
                        or.MustBe(typeof(int));
                        or.MustBe(typeof(string));
                    });

            builder.MustSatisfy(m => m.IsAbstract ? m.Parameters.Count > 1 : m.IsSealed, m => $"{m} must be magic");
        }
    }
}
