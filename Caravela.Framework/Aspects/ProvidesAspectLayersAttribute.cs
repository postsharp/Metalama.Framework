using System;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{

    /// <summary>
    /// Custom attribute that, when added on a class implementing <see cref="IAspect{T}"/>, allows the aspect
    /// to provide advices into different aspect layers by calling the <see cref="IAdviceFactory.ForLayer"/> method.
    /// Aspect layers are executed in the ordered specified in the attribute constructor, aspect layers of different
    /// aspect types can execute between two parts of the same aspect type. Note that the default layer
    /// (specified by a null or empty string) is always included by default and cannot be specified in this
    /// custom attribute.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class ProvidesAspectLayersAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProvidesAspectLayersAttribute"/> class.
        /// </summary>
        /// <param name="layers">An ordered list of aspect parts, which cannot include an empty or null string that denotes
        /// the default layer.</param>
        public ProvidesAspectLayersAttribute( params string[] layers )
        {
            this.Layers = layers;
        }

        /// <summary>
        /// Gets the list of aspect parts specified in the constructor.
        /// </summary>
        public IReadOnlyList<string> Layers { get; }
    }
}