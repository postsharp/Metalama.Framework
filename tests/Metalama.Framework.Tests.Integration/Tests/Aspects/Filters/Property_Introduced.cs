using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Property_Introduced;

#pragma warning disable CS8618, CS0169

[assembly: AspectOrder(typeof(IntroduceAndFilterAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Property_Introduced
{
    /*
     * Tests that filter works on introduced property within the same aspect.
     */

    internal class IntroduceAndFilterAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advice.AddContract(property, nameof(Filter), ContractDirection.Both);
            }

            var introducedField = builder.Advice.IntroduceField(builder.Target, nameof(IntroducedProperty));

            builder.Advice.AddContract(introducedField, nameof(Filter), ContractDirection.Both);
        }

        [Template]
        public string? IntroducedProperty { get; set; }

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
        public string? ExistingProperty { get; set; }
    }
}