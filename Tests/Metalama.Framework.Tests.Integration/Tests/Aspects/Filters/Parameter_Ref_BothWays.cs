using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.RefParameter_Both;

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