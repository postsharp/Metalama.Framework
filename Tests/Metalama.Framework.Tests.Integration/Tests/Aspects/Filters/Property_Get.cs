using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.PropertyOutput
{
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter( dynamic? value )
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
        private string q;

        [NotNull]
        public string P => "p";

        [NotNull]
        public string Q
        {
            get
            {
                return q;
            }
        }
    }
}