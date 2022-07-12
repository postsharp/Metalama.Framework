using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Both
{
    internal class NotNullAttribute : ContractAspect
    {
        public NotNullAttribute() : base( ContractDirection.Both )
        {
            
        }
        
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
        [NotNull]
        private string q;
    }

    // <target>
    internal struct TargetStruct
    {
        [NotNull]
        private string q;
    }
}