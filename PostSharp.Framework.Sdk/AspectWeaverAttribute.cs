using System;

namespace PostSharp.Framework.Sdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectWeaverAttribute : Attribute
    {
        public Type AspectType { get; }

        public AspectWeaverAttribute(Type aspectType) => AspectType = aspectType;
    }
}
