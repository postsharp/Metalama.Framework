using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS0169, CS0649

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.AutoProperty_Get_Inheritance
{
    [Inheritable]
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

    internal interface ITarget
    {
        [NotNull]
        string P { get; }
    }

    // <target>
    internal class Target : ITarget
    {
        public string P { get; } = null!;

        public Target()
        {
            this.P = "42";
        }
    }
}