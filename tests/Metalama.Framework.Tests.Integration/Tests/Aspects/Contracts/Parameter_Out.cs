using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Parameter_Out
{
    internal class NotNullAttribute : ContractAspect
    {
        public override void Validate( dynamic? value )
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
        private void M( [NotNull] out string m )
        {
            m = "";
        }
    }

    // <target>
    internal record TargetRecord
    {
        private void M([NotNull] out string m)
        {
            m = "";
        }
    }

    // <target>
    internal struct TargetStruct
    {
        private void M([NotNull] out string m)
        {
            m = "";
        }
    }
}