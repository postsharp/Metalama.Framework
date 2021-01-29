using Caravela.Framework.Project;
using Caravela.Reactive;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent an element of code. Implementations of <see cref="ICodeElement"/>
    /// are always declarations, never elements of the method body.
    /// </summary>
    [CompileTime]
    public interface ICodeElement : IDisplayable
    {
        /// <summary>
        /// Containing element of code, such as a <see cref="INamedType"/> for nested
        /// types or for methods. If the containing element is a namespace or
        /// a compilation, <c>null</c> is returned.  
        /// </summary>
        ICodeElement? ContainingElement { get; }
        
        /// <summary>
        /// Gets the collection of custom attributes on the element of code.
        /// </summary>
        IReactiveCollection<IAttribute> Attributes { get; }

        /// <summary>
        /// Gets the kind of element of code.
        /// </summary>
        public CodeElementKind Kind { get; }
    }
}