using System.Collections.Generic;

namespace Caravela.Framework
{
    internal class AspectType
    {
        public string Name { get; }
        public IAspectDriver AspectDriver { get; }
        public IReadOnlyList<AspectPart> Parts { get; }

        public AspectType(string name, IAspectDriver aspectDriver, IReadOnlyList<AspectPart> parts)
        {
            Name = name;
            AspectDriver = aspectDriver;
            Parts = parts;
        }
    }
}
