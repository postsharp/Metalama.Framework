using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.ExpressionBody_Throw
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
    internal partial class Target
    {
        public void M1([NotNull] string m) => throw new Exception();

        public int M2([NotNull] string m) => throw new Exception();
    }
}