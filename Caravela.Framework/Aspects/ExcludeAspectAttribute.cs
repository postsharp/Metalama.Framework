using System;

namespace Caravela.Framework.Aspects
{
    
    /// <summary>
    /// Custom attribute attribute that, when applied to an element of code, specifies that this element of code must not be
    /// the target of aspects of given types.
    /// (Not implemented.)
    /// </summary>
    [Obsolete("Not implemented.")]
    [AttributeUsage(AttributeTargets.All)]
    public class ExcludeAspectAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAspectAttribute"/> class.
        /// </summary>
        /// <param name="excludedAspectTypes"></param>
        [Obsolete( "Not implemented." )]
        public ExcludeAspectAttribute( params Type[] excludedAspectTypes )
        {
            _ = excludedAspectTypes;
        }
    }
}