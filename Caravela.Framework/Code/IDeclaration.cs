// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent a declaration. Implementations of <see cref="IDeclaration"/>
    /// are always declarations, never elements of the method body.
    /// </summary>
    [CompileTimeOnly]
    public interface IDeclaration : IDisplayable, IDiagnosticScope, ICompilationElement
    {
        /// <summary>
        /// Gets the origin (<see cref="CodeOrigin.Source"/>, <see cref="CodeOrigin.Generator"/> or <see cref="CodeOrigin.Aspect"/>
        /// of the current declaration.
        /// </summary>
        CodeOrigin Origin { get; }

        /// <summary>
        /// Gets the containing declaration, such as a <see cref="INamedType"/> for nested
        /// types or for methods. If the containing element is a namespace or
        /// a compilation, <c>null</c> is returned.
        /// </summary>
        IDeclaration? ContainingDeclaration { get; }

        /// <summary>
        /// Gets the collection of custom attributes on the declaration.
        /// </summary>
        IAttributeList Attributes { get; }

        /// <summary>
        /// Gets the kind of declaration.
        /// </summary>
        public DeclarationKind DeclarationKind { get; }
    }
}