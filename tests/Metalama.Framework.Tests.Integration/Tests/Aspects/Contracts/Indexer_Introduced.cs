using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Indexer_Introduced;

#pragma warning disable CS8618, CS0169

[assembly: AspectOrder( typeof(IntroduceAndFilterAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Indexer_Introduced
{
    /*
     * Tests that filter works on introduced property within the same aspect.
     */

    internal class IntroduceAndFilterAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advise.AddContract( property, nameof(Filter), ContractDirection.Both );
            }

            var introducedField = builder.Advise.IntroduceProperty( builder.Target, nameof(IntroducedProperty) ).Declaration;

            builder.Advise.AddContract( introducedField, nameof(Filter), ContractDirection.Both );
        }

        [Template]
        public string? IntroducedProperty { get; set; }

        [Template]
        public void Filter( dynamic? value )
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