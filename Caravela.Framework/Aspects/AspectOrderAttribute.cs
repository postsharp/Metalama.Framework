using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that specifies the order of evaluation of aspects or aspect layers.
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    public class AspectOrderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AspectOrderAttribute"/> class that specifies the order of execution
        /// of aspects. This constructor does not allow multi-layer aspects to overlap each other. If aspects are composed
        /// of several layers, all layers of each aspect are ordered as a single group. To order layers individually, use
        /// the other constructor.
        /// </summary>
        /// <param name="orderedAspectTypes">A list of aspect types given the desired order of execution.</param>
        public AspectOrderAttribute( params Type[] orderedAspectTypes )
        {
            this.OrderedAspectLayers = orderedAspectTypes.Select( t => t.FullName + ":*" ).ToArray();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectOrderAttribute"/> class that specified the order of execution
        /// of aspect layers. This constructor allows to specify the order of execution of individual layers.
        /// </summary>
        /// <param name="orderedAspectLayers">A list of layer names composed of the full name of the aspect type and the name
        /// of the aspect layer. The following formats are allowed: <c>MyNamespace.MyAspectType</c> to match the default layer,
        /// <c>MyNamespace.MyAspectType:MyLayer</c> to match a non-default layer, or <c>MyNamespace.MyAspectType:*</c> to match
        /// all layers of an aspect.
        /// 
        /// </param>
        public AspectOrderAttribute( params string[] orderedAspectLayers )
        {
            this.OrderedAspectLayers = orderedAspectLayers;
        }

        /// <summary>
        /// Gets the ordered list of aspect layers, in the format specified the constructor documentation.
        /// </summary>
        public IReadOnlyList<string> OrderedAspectLayers { get; }
    }
}