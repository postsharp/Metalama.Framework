using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Property_InvalidDirections
{
    internal class NotNullAttribute : FilterAspect
    {
        public NotNullAttribute( FilterDirection direction ) : base( direction ) { }

        public override void Filter( dynamic? value )
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
        }
    }

    internal class Target
    {
        private string q;

        // All these targets are invalid.

        [NotNull( FilterDirection.Input )]
        public string P1 => "";

        [NotNull( FilterDirection.Both )]
        public string P2 => "";

        [NotNull( FilterDirection.Output )]
        public string P3
        {
            set { }
        }
    }
}