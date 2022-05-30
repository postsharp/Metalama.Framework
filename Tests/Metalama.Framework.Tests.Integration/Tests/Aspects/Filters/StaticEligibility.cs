using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Parameter_StaticEligibility;

internal class NotNullAttribute : ContractAspect
{
    public NotNullAttribute() : base( ContractDirection.Input ) { }

    public override void BuildEligibility( IEligibilityBuilder<IParameter> builder ) => BuildEligibilityForDirection( builder, ContractDirection.Input );

    public override void BuildEligibility( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder )
        => BuildEligibilityForDirection( builder, ContractDirection.Input );

    public override void Validate( dynamic? value )
    {
        if (value == null)
        {
            throw new ArgumentNullException( meta.Target.Parameter.Name );
        }
    }
}

// <target>
internal class Target
{
    private void M( [NotNull] out string x )
    {
        x = "";
    }

    [NotNull]
    private string P => "";
}