using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Parameter_Ref_BothWays;

#pragma warning disable CS8618

internal class NotNullAttribute : ContractAspect
{
    public NotNullAttribute() : base( ContractDirection.Both ) { }

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
    private void M( [NotNull] ref string m )
    {
        m = "";
    }
}