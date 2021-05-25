// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// An annotation is an object that can be applied to a declaration and that provides information to an aspect.
    /// Annotations are exposed on the <see cref="IDeclaration.GetAnnotations{T}"/> property of the <see cref="IDeclaration"/> interface.
    /// This interface must not be implemented directly by users. Users should implement the strongly-typed <see cref="IAnnotation{TTarget,TAspect}"/>
    /// interface.
    /// </summary>
    [CompileTimeOnly]
    [Obsolete( "Not implemented." )]
    public interface IAnnotation { }

    /// <summary>
    /// An annotation is an object that can be applied to a declaration and that provides information to an aspect.
    /// Annotations are exposed on the <see cref="IDeclaration.GetAnnotations{T}"/> property of the <see cref="IDeclaration"/> interface.
    /// </summary>
    /// <typeparam name="TTarget">The type of declarations on which the declaration can be added.</typeparam>
    /// <typeparam name="TAspect">The type of aspects for which the annotation is meaningful.</typeparam>
    [CompileTimeOnly]
    [Obsolete( "Not implemented." )]
    public interface IAnnotation<TTarget, TAspect> : IEligible<TTarget>, IAnnotation
        where TAspect : IAspect
        where TTarget : class, IDeclaration { }
}