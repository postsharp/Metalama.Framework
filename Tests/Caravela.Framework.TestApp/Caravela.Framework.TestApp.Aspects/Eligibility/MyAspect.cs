using System;
using System.Collections.Generic;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.TestApp.Aspects.Eligibility
{
    // Example of an aspect with complex eligibility rules.
    public class MyAspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect(IAspectBuilder<IMethod> builder) => throw new NotImplementedException();

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
            builder.ExceptForInheritance().MustBeNonAbstract();

            builder.Parameter(0)
                .Type()
                .MustSatisfyAny(
                    or =>
                    {
                        or.MustBe(typeof(int));
                        or.MustBe(typeof(string));
                    });

            builder.Require(m => m.IsAbstract ? m.Parameters.Count > 1 : m.IsSealed, m => $"{m} must be magic");
        }
    }
}
