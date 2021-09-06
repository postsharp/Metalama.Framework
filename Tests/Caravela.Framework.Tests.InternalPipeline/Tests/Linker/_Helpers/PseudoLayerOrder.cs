using System;

namespace Caravela.Framework.Tests.Integration.Tests.Linker
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class PseudoLayerOrder : Attribute
    {
        public PseudoLayerOrder(string aspectName, string? layerName = null)
        {
        }
    }
}
