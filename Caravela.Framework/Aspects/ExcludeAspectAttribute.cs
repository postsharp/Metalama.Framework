using System;

namespace Caravela.Framework.Aspects
{
    
    [Obsolete("Not implemented.")]
    public class ExcludeAspectAttribute : Attribute
    {
        [Obsolete( "Not implemented." )]
        public ExcludeAspectAttribute( params Type[] excludedAspectTypes )
        {
            _ = excludedAspectTypes;
        }
    }
}