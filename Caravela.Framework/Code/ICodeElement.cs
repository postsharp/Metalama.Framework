using Caravela.Framework.Project;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent an element of code. Implementations of <see cref="ICodeElement"/>
    /// are always declarations, never elements of the method body.
    /// </summary>
    [CompileTime]
    public interface ICodeElement : IDisplayable, IEquatable<ICodeElement>
    {
        /// <summary>
        /// Gets the containing element of code, such as a <see cref="INamedType"/> for nested
        /// types or for methods. If the containing element is a namespace or
        /// a compilation, <c>null</c> is returned.
        /// </summary>
        ICodeElement? ContainingElement { get; }

        /// <summary>
        /// Gets the collection of custom attributes on the element of code.
        /// </summary>
        IReadOnlyList<IAttribute> Attributes { get; }

        /// <summary>
        /// Gets the kind of element of code.
        /// </summary>
        public CodeElementKind ElementKind { get; }
    }
}