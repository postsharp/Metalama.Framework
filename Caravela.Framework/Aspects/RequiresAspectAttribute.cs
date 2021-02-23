using System;

namespace Caravela.Framework.Aspects
{

    [Obsolete("Not implemented.")]
    public sealed class RequiresAspectAttribute : Attribute
    {
        [Obsolete( "Not implemented." )]
        public RequiresAspectAttribute( params Type[] requiredAspectTypes )
        {
            _ = requiredAspectTypes;
        }
    }
}