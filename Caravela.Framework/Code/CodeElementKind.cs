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
        /// Not a valid code element represented by <see cref="ICodeElement"/>.
        /// </summary>
        None,

        /// <summary>
        /// <see cref="ICompilation"/>.
        /// </summary>
        Compilation,

        /// <summary>
        /// <see cref="INamedType"/>.
        /// </summary>
        Type,

        /// <summary>
        /// <see cref="IMethod"/>.
        /// </summary>
        Method,

        /// <summary>
        /// <see cref="IProperty"/>.
        /// </summary>
        Property,

        /// <summary>
        /// <see cref="IProperty"/> (fields are represented as properties).
        /// </summary>
        Field,

        /// <summary>
        /// <see cref="IEvent"/>.
        /// </summary>
        Event,

        /// <summary>
        /// <see cref="IParameter"/>.
        /// </summary>
        Parameter,

        /// <summary>
        /// <see cref="IGenericParameter"/>.
        /// </summary>
        GenericParameter,

        /// <summary>
        /// <see cref="IAttribute"/>.
        /// </summary>
        Attribute,

        /// <summary>
        /// <see cref="IManagedResource"/>.
        /// </summary>
        ManagedResource,

        /// <summary>
        /// <see cref="IConstructor"/>.
        /// </summary>
        Constructor,
        
        /// <summary>
        /// A reference assembly, implementing <see cref="IAssembly"/>. Note
        /// that the current assembly is represented by <see cref="ICompilation"/> that inherits <see cref="IAssembly"/>, but the
        /// <see cref="CodeElementKind"/> for the current compilation is <see cref="Compilation"/> and not <see cref="ReferencedAssembly"/>. 
        /// </summary>
        ReferencedAssembly,
    }
}