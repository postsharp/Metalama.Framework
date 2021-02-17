using System;
using System.Collections.Generic;
using Caravela.Framework.Advices;

namespace Caravela.Framework.Aspects
{

    /// <summary>
    /// Custom attribute that, when added on a class implementing <see cref="IAspect{T}"/>, allows the aspect
    /// to provide advices into different aspect parts by setting the <see cref="IAdvice.PartName"/> property.
    /// Aspect parts are executed in the ordered specified in the attribute constructor, aspect parts of different
    /// aspect types can execute between two parts of the same aspect type. Note that the default part
    /// (specified by a null or empty string) is always included by default.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class ProvidesAspectPartsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidesAspectPartsAttribute"/> class.
        /// </summary>
        /// <param name="parts">An ordered list of aspect parts.</param>
        public ProvidesAspectPartsAttribute( params string[] parts )
        {
            this.Parts = parts;
        }

        /// <summary>
        /// Gets the list of aspect parts specified in the constructor.
        /// </summary>
        public IReadOnlyList<string> Parts { get; }
    }
}