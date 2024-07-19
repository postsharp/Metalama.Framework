using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.OverriddenProperty_Half;

#pragma warning disable CS8618

internal class NotNullAttribute : ContractAspect
{
    public ContractDirection Direction { get; set; }

    protected override ContractDirection GetDefinedDirection( IAspectBuilder builder ) => Direction;

    public override void Validate( dynamic? value )
    {
        if (value == null)
        {
            throw new ArgumentNullException();
        }
    }
}

internal class B
{
    [NotNull]
    public virtual string Default { get; set; }

    [NotNull( Direction = ContractDirection.Both )]
    public virtual string Both { get; set; }

    [NotNull( Direction = ContractDirection.Input )]
    public virtual string Input { get; set; }

    [NotNull( Direction = ContractDirection.Output )]
    public virtual string Output { get; set; }
}

// <target>
internal class C : B
{
    public override string Default => "C";

    public override string Both => "C";

    public override string Input => "C";

    public override string Output => "C";
}