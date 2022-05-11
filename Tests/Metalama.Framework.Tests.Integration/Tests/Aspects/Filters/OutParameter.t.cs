using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.OutParameter
{
#pragma warning disable CS0067
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    internal class Target
    {
        private void M( [NotNull] out string m )
        {
                m = "";
    if (m == null)
    {
        throw new global::System.ArgumentNullException("m");
    }
        }
    }
}