using System;

namespace Caravela.Framework.Aspects
{

    /// <summary>
    /// Custom attribute that, when applied to an aspect type, specifies that all instances of the target aspect type
    /// require an instance of other given aspect types. These aspect types must have a public default constructor.
    /// (Not implemented.)
    /// </summary>
    [Obsolete("Not implemented.")]
    public sealed class RequiresAspectAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresAspectAttribute"/> class.
        /// </summary>
        /// <param name="requiredAspectTypes">List of required aspect types. These types must have a public default constructor.</param>
        [Obsolete( "Not implemented." )]
        public RequiresAspectAttribute( params Type[] requiredAspectTypes )
        {
            _ = requiredAspectTypes;
        }
    }
}