using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Introduced;

#pragma warning disable CS8618, CS0169, CS0649

[assembly: AspectOrder( typeof(IntroduceAndFilterAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Introduced
{
    /*
     * Tests that filter works on introduced field within the same aspect.
     */

    internal class IntroduceAndFilterAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var field in builder.Target.Fields)
            {
                builder.Advise.AddContract( field, nameof(Filter), ContractDirection.Both );
            }

            var introducedField = builder.Advise.IntroduceField( builder.Target, nameof(IntroducedField) ).Declaration;

            builder.Advise.AddContract( introducedField, nameof(Filter), ContractDirection.Both );
        }

        [Template]
        private string? IntroducedField;

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
        private string ExistingField;
    }
}