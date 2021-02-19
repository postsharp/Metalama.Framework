using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.AspectOrdering
{
    class AspectOrderSpecification
    {

        public AspectOrderSpecification( AspectOrderAttribute attribute, Location location )
        {
            this.OrderedParts = attribute.OrderedAspectParts;
            this.DiagnosticLocation = location;
        }
        
        public Location? DiagnosticLocation { get; }
        public IReadOnlyList<string> OrderedParts { get; }
    }
}