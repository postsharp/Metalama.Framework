using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Parameter_ReturnVoid
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
        [return: NotNull]
        private void M()
        {
        }
    }
}