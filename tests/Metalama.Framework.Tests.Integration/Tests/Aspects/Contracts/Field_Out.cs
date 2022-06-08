using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Out
{
    internal class NotNullAttribute : ContractAspect
    {
        public NotNullAttribute() : base( ContractDirection.Output )
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
}