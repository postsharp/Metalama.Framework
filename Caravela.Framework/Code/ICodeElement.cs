// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent an element of code. Implementations of <see cref="ICodeElement"/>
    /// are always declarations, never elements of the method body.
    /// </summary>
    [CompileTime]
    public interface ICodeElement : IDisplayable, IDiagnosticScope, ICompilationElement
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
    }
}