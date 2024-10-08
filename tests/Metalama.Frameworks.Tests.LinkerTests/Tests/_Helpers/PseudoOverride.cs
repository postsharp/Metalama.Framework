using System;

namespace Metalama.Framework.Tests.Integration.Tests.Linker
{
    [AttributeUsage(AttributeTargets.All)]
    public class PseudoOverride : Attribute
    {
        public PseudoOverride(string targetMember, string aspectName, string? layerName= null)
        {
        }
    }
}
