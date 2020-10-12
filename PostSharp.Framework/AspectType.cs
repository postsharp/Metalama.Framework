using System.Collections.Generic;

namespace PostSharp.Framework.Sdk
{
    internal class AspectType
    {
        public IAspectDriver AspectDriver { get; }
        public IReadOnlyList<AspectPart> Parts { get; }
    }

    internal class AspectPart
    {
        public string? Name { get; }
        public int ExecutionOrder { get; }
    }
}
