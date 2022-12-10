// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An annotation is an object that can be applied to a declaration and that provides information to an aspect.
    /// Annotations are exposed by the <see cref="DeclarationExtensions.Enhancements"/> extension method of the <see cref="IDeclaration"/> interface.
    /// This interface must not be implemented directly by users. Users should implement the strongly-typed <see cref="IAnnotation{TTarget,TAspect}"/>
    /// interface.
    /// (Not implemented.)
    /// </summary>
    [CompileTime]
    [Obsolete( "Not implemented." )]
    public interface IAnnotation { }

    // ReSharper disable UnusedTypeParameter

    /// <summary>
    /// An annotation is an object that can be applied to a declaration and that provides information to an aspect.
    /// Annotations are exposed by the <see cref="DeclarationExtensions.Enhancements"/> extension method of the <see cref="IDeclaration"/> interface.
    /// (Not implemented.)
    /// </summary>
    /// <typeparam name="TTarget">The type of declarations on which the declaration can be added.</typeparam>
    /// <typeparam name="TAspect">The type of aspects for which the annotation is meaningful.</typeparam>
    [CompileTime]
    [Obsolete( "Not implemented." )]
    public interface IAnnotation<in TTarget, TAspect> : IEligible<TTarget>, IAnnotation
        where TAspect : IAspect
        where TTarget : class, IDeclaration { }
}