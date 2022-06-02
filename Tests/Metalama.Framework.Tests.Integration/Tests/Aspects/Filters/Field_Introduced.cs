using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Field_Introduced;

#pragma warning disable CS8618, CS0169

[assembly: AspectOrder(typeof(IntroduceAndFilterAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Field_Introduced
{
    /*
     * Tests that filter works on introduced field within the same aspect.
     */

    internal class IntroduceAndFilterAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var field in builder.Target.Fields)
            {
                builder.Advice.AddFilter(field, nameof(Filter), FilterDirection.Both);
            }

            var introducedField = builder.Advice.IntroduceField(builder.Target, nameof(IntroducedField));

            builder.Advice.AddFilter(introducedField, nameof(Filter), FilterDirection.Both);
        }

        [Template]
        private string? IntroducedField;

        [Template]
        public void Filter(dynamic? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
        }
    }

    // <target>
    [IntroduceAndFilter]
    internal class Target
    {
        private string ExistingField;
    }
}