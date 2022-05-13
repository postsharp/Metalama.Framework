using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618 

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.PropertyInput
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
        public string P { get; set; }

        [NotNull]
        public string Q
        {
            get => q;
            set => q = value + "-";
        }
    }
}