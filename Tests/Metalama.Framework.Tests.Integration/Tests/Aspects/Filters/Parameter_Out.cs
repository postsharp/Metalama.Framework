using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.OutParameter
{
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter( dynamic? value )
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
}