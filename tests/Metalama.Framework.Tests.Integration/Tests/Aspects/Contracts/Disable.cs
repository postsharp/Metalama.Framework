using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.NoneDirection;

internal class NotNullAttribute : ContractAspect
{
    protected override bool IsEnabled( IAspectBuilder builder, ContractDirection direction ) => false;

    public override void Validate( dynamic? value )
    {
        if (value == null)
        {
            throw new ArgumentNullException();
        }
    }
}

// <target>
internal class C
{
    [NotNull]
    public string? P { get; set; }
}