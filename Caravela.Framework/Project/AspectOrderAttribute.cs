using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Project
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AspectOrderAttribute : Attribute
    {
        public AspectOrderAttribute( params Type[] orderedAspectTypes )
        {
            this.OrderedAspectLayers = orderedAspectTypes.Select( t => t.FullName + ":*" ).ToArray();
        }

        public AspectOrderAttribute( params string[] orderedAspectLayers )
        {
            this.OrderedAspectLayers = orderedAspectLayers;
        }

        public IReadOnlyList<string> OrderedAspectLayers { get; }
        
        
    }
}