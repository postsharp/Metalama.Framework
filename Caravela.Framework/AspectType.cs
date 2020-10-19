using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework
{
    internal class AspectType
    {
        public string Name { get; }
        public IAspectDriver AspectDriver { get; }
        public IImmutableList<AspectPart> Parts { get; }

        public AspectType(string name, IAspectDriver aspectDriver, IImmutableList<AspectPart> parts)
        {
            this.Name = name;
            this.AspectDriver = aspectDriver;
            this.Parts = parts;
        }
    }
}
