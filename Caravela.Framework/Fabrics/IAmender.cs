// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Fabrics
{
    /// <summary>
    /// Base interface for the argument of <see cref="IProjectFabric.AmendProject"/>, <see cref="INamespaceFabric.AmendNamespace"/>
    /// or <see cref="ITypeFabric.AmendType"/>. Allows to report diagnostics and add aspects to the target declaration of the fabric.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAmender<T>
        where T : class, IDeclaration
    {
        /// <summary>
        /// Gets the project being built.
        /// </summary>
        IProject Project { get; }

        /// <summary>
        /// Creates an object that allows adding aspects to members of the target declaration.
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TChild"></typeparam>
        /// <returns></returns>
        IDeclarationSelection<TChild> WithMembers<TChild>( Func<T, IEnumerable<TChild>> selector )
            where TChild : class, IDeclaration;

        /// <summary>
        /// Registers a validator, which gets executed after all aspects have been added to the compilation.
        /// </summary>
        /// <param name="validator"></param>
        [Obsolete( "Not implemented." )]
        void AddValidator( Action<ValidateDeclarationContext<T>> validator );

        /// <summary>
        /// Registers a rule that can provide annotations for a given aspect type, on demand.
        /// </summary>
        /// <param name="provider">A delegate that returns an annotation instance, or <c>null</c> if no annotation is needed.</param>
        /// <typeparam name="TTarget">The type of the target of the annotation.</typeparam>
        /// <typeparam name="TAspect">The type of the aspect that consumes the annotation. If the annotation targets several
        /// aspects, then <typeparamref name="TAspect"/> must be the base type or an interface.</typeparam>
        /// <typeparam name="TAnnotation">The type of the annotation.</typeparam>
        [Obsolete( "Not implemented." )]
        void AddAnnotation<TTarget, TAspect, TAnnotation>( Func<TTarget, TAnnotation?> provider )
            where TAnnotation : IAnnotation<TTarget, TAspect>
            where TAspect : IAspect
            where TTarget : class, IDeclaration;
    }
}