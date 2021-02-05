using Caravela.Framework.Sdk;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class AspectType
    {
        public string Name { get; }
        public IAspectDriver AspectDriver { get; }
        public IImmutableList<AspectPart> Parts { get; }

        public AspectType(string name, IAspectDriver aspectDriver, IEnumerable<string?> partNames)
        {
            this.Name = name;
            this.AspectDriver = aspectDriver;
            this.Parts = partNames.Select( partName => new AspectPart( this, partName ) ).ToImmutableList();
        }
    }
}
