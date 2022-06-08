using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0169, CS0649

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Property_Get
{
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
    internal class Target
    {
        private string q;

        [NotNull]
        public string P => "p";

        [NotNull]
        public string Q
        {
            get
            {
                return q;
            }
        }
    }
}