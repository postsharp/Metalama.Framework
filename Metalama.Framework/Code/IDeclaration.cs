// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Metrics;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represent a declaration.
    /// </summary>
    /// <seealso cref="DeclarationExtensions"/>
    [CompileTime]
    public interface IDeclaration : IDisplayable, IDiagnosticLocation, ICompilationElement, IMeasurable
    {
        /// <summary>
        /// Gets a reference to the compilation, which can be used to identify the current declaration
        /// in a different revision of the compilation.
        /// </summary>
        /// <returns></returns>
        IRef<IDeclaration> ToRef();

        /// <summary>
        /// Gets the declaring assembly, which can be the current <see cref="ICompilationElement.Compilation"/>
        /// or a reference assembly.
        /// </summary>
        IAssembly DeclaringAssembly { get; }

        /// <summary>
        /// Gets the origin (<see cref="DeclarationOrigin.Source"/>, <see cref="DeclarationOrigin.Generator"/> or <see cref="DeclarationOrigin.Aspect"/>
        /// of the current declaration.
        /// </summary>
        DeclarationOrigin Origin { get; }

        /// <summary>
        /// Gets the containing declaration, such as a <see cref="INamedType"/> for nested
        /// types or for methods. If the containing element is a namespace or
        /// a compilation, <c>null</c> is returned.
        /// </summary>
        IDeclaration? ContainingDeclaration { get; }

        /// <summary>
        /// Gets the collection of custom attributes on the declaration.
        /// </summary>
        IAttributeCollection Attributes { get; }

        /// <summary>
        /// Gets the kind of declaration.
        /// </summary>
        public DeclarationKind DeclarationKind { get; }

        /// <summary>
        /// Gets a value indicating whether the member is implicitly declared, i.e. declared without being represented in source code.
        /// Returns <c>false</c> if it is explicitly declared in code.
        /// </summary>
        bool IsImplicitlyDeclared { get; }
    }
}