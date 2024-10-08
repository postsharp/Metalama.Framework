using System;

namespace Metalama.Framework.Tests.Integration.Tests.Linker
{
    [AttributeUsage(AttributeTargets.All)]
    public class PseudoIntroduction : Attribute
    {
        public PseudoIntroduction(string aspectName, string? layerName = null)
        {
        }
    }
}
