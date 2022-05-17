using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.RefParameter
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
        private void M( [NotNull] ref string m )
        {
            m = "";
        }
    }
}