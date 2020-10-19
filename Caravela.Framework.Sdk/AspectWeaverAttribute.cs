using System;

namespace Caravela.Framework.Sdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectWeaverAttribute : Attribute
    {
        public Type AspectType { get; }

        public AspectWeaverAttribute(Type aspectType) => this.AspectType = aspectType;
    }
}
