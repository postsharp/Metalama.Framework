using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.OverriddenProperty_AlternatingHalves;

#pragma warning disable CS8618

internal class NotNullAttribute : ContractAspect
{
    public override void Validate( dynamic? value )
    {
        if (value == null)
        {
            throw new ArgumentNullException();
        }
    }
}

// <target>
class B
{
    public virtual string P { get; set; }
}

// <target>
class C : B
{
    [NotNull]
    public override string P => "C";
}

// <target>
class C2 : C
{
    public override string P { set { } }
}