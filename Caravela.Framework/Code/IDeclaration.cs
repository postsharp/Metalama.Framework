// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Diagnostics;
using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represent a declaration.
    /// </summary>
    [CompileTimeOnly]
    public interface IDeclaration : IDisplayable, IDiagnosticScope, ICompilationElement
    {
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
        IAttributeList Attributes { get; }

        /// <summary>
        /// Gets the kind of declaration.
        /// </summary>
        public DeclarationKind DeclarationKind { get; }

        /// <summary>
        /// Determines whether an aspect of a specified type has been added to the current declaration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete( "Not implemented." )]
        bool HasAspect<T>()
            where T : IAspect;

        /// <summary>
        /// Gets the list of annotations registered on the current declaration for a given aspect type.
        /// </summary>
        /// <typeparam name="T">The type of the aspect for which the annotations are requested.</typeparam>
        [Obsolete( "Not implemented." )]
        IAnnotationList GetAnnotations<T>()
            where T : IAspect;
    }
}