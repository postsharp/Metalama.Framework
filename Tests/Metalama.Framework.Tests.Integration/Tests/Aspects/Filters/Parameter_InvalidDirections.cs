using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.InvalidDirections
{
    internal class NotNullAttribute : FilterAspect
    {
        public NotNullAttribute( FilterDirection direction ) : base( direction ) { }

        public override void Filter( dynamic? value )
        {
            if (value == null)
            {
                throw new ArgumentNullException( meta.Target.Parameter.Name );
            }
        }
    }

    internal class Target
    {
        // All these situations are invalid and should result in eligibility errors.

        private void M1( [NotNull( FilterDirection.Output )] string m ) { }

        private void M2( [NotNull( FilterDirection.Both )] string m ) { }

        private void M3( [NotNull( FilterDirection.Input )] out string m )
        {
            m = "";
        }

        private void M4( [NotNull( FilterDirection.Both )] out string m )
        {
            m = "";
        }
    }
}