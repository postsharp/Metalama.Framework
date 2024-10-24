#if TEST_OPTIONS
// In C# 10, we generate slightly different code.
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
# endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Field_Both
{
    internal class NotNullAttribute : ContractAspect
    {
        protected override ContractDirection GetDefinedDirection( IAspectBuilder builder ) => ContractDirection.Both;

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

        public TargetStruct( string q )
        {
            this.q = q;
        }
    }
}