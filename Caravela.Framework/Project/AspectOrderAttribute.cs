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
            this.OrderedAspectParts = orderedAspectTypes.Select( t => t.FullName + ":*" ).ToArray();
        }

        public AspectOrderAttribute( params string[] orderedAspectParts )
        {
            this.OrderedAspectParts = orderedAspectParts;
        }

        public IReadOnlyList<string> OrderedAspectParts { get; }
        
        
    }
}