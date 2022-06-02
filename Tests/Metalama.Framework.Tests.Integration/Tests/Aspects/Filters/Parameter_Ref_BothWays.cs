using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Parameter_Ref_BothWays;

#pragma warning disable CS8618

internal class NotNullAttribute : FilterAspect
{
    public NotNullAttribute() : base( FilterDirection.Both ) { }

    public override void Filter( dynamic? value )
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