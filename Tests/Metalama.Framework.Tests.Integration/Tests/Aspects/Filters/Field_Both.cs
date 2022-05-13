using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.Field_Both
{
    internal class NotNullAttribute : FilterAspect
    {
        public NotNullAttribute() : base( FilterDirection.Both )
        {
            
        }
        
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
        [NotNull]
        private string q;
    }
}