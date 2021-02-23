using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent an element of code. Implementations of <see cref="ICodeElement"/>
    /// are always declarations, never elements of the method body.
    /// </summary>
    [CompileTime]
    public interface ICodeElement : IDisplayable, IDiagnosticTarget
    {
        /// <summary>
        /// Gets the origin (<see cref="CodeOrigin.Source"/>, <see cref="CodeOrigin.Generator"/> or <see cref="CodeOrigin.Aspect"/>
        /// of the current code element.
        /// </summary>
        CodeOrigin Origin { get; }

        /// <summary>
        /// Gets the containing element of code, such as a <see cref="INamedType"/> for nested
        /// types or for methods. If the containing element is a namespace or
        /// a compilation, <c>null</c> is returned.
        /// </summary>
        ICodeElement? ContainingElement { get; }

        /// <summary>
        /// Gets the collection of custom attributes on the element of code.
        /// </summary>
        IAttributeList Attributes { get; }

        /// <summary>
        /// Gets the kind of element of code.
        /// </summary>
        public CodeElementKind ElementKind { get; }

        ICompilation Compilation { get; }
    }

    /// <summary>
    /// Origins of an element of code.
    /// </summary>
    public enum CodeOrigin
    {
        /// <summary>
        /// Source code.
        /// </summary>
        Source,

        /// <summary>
        /// Roslyn code generator.
        /// </summary>
        Generator,

        /// <summary>
        /// Aspect (introduction).
        /// </summary>
        Aspect
    }
}