using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Kinds of a <see cref="ICodeElement"/>.
    /// </summary>
    [CompileTime]
    public enum CodeElementKind
    {
        /// <summary>
        /// <see cref="ICompilation"/>.
        /// </summary>
        Compilation,

        /// <summary>
        /// <see cref="INamedType"/>.
        /// </summary>
        Type,

        /// <summary>
        /// <see cref="IMethod"/>
        /// </summary>
        Method,

        /// <summary>
        /// <see cref="IProperty"/>
        /// </summary>
        Property,

        /// <summary>
        /// <see cref="IProperty"/> (fields are represented as properties).
        /// </summary>
        Field,

        /// <summary>
        /// <see cref="IEvent"/>
        /// </summary>
        Event,

        /// <summary>
        /// <see cref="IParameter"/>
        /// </summary>
        Parameter,

        /// <summary>
        /// <see cref="GenericParameter"/>
        /// </summary>
        GenericParameter,
        
        Attribute,
        Resource
    }
}