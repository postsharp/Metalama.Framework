using System;

namespace PostSharp.Framework.Sdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AspectDriverAttribute : Attribute
    {
        public Type AspectType { get; }

        public AspectDriverAttribute(Type aspectType) => AspectType = aspectType;
    }
}
