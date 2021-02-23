using System;

namespace Caravela.Framework.Sdk
{

    /// <summary>
    /// Custom attribute that, when applied to a type, exports it to the collection of compiler plug-ins. Aspect weavers are plug-ins
    /// and must be annotated with this custom attribute.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class CompilerPluginAttribute : Attribute
    {
    }
}
