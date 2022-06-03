using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Property_InvalidDirections
{
    internal class NotNullAttribute : ContractAspect
    {
        public NotNullAttribute( ContractDirection direction ) : base( direction ) { }

        public override void Validate( dynamic? value )
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
        }
    }

    internal class Target
    {
        private string q;

        // All these targets are invalid.

        [NotNull( ContractDirection.Input )]
        public string P1 => "";

        [NotNull( ContractDirection.Both )]
        public string P2 => "";

        [NotNull( ContractDirection.Output )]
        public string P3
        {
            set { }
        }
    }
}