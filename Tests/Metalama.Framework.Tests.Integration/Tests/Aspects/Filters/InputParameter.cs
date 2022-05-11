using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.InputParameter
{
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter(dynamic? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }

    internal class Target
    {
        void M([NotNull] string m)
        {

        }
    }
}
