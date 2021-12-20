// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects;

/// <summary>
/// An interface that allows aspects and fabrics to add child aspects and register validators
/// </summary>
public interface IDeclarationSelector<out TTarget>
    where TTarget : class, IDeclaration
{
    /// <summary>
    /// Selects members of the  target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IDeclarationSelection{TDeclaration}.AddAspect{TAspect}(System.Func{TDeclaration,System.Linq.Expressions.Expression{System.Func{TAspect}}})"/>,
    ///  <see cref="IDeclarationSelection{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>,  <see cref="IDeclarationSelection{TDeclaration}.RegisterDeclarationValidator{T}"/>
    /// or <see cref="IDeclarationSelection{TDeclaration}.RegisterReferenceValidator"/>.
    /// </summary>
    IDeclarationSelection<TMember> WithTargetMembers<TMember>( Func<TTarget, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects the  target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IDeclarationSelection{TDeclaration}.AddAspect{TAspect}(System.Func{TDeclaration,System.Linq.Expressions.Expression{System.Func{TAspect}}})"/>,
    ///  <see cref="IDeclarationSelection{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>,  <see cref="IDeclarationSelection{TDeclaration}.RegisterDeclarationValidator{T}"/>
    /// or <see cref="IDeclarationSelection{TDeclaration}.RegisterReferenceValidator"/>.
    /// </summary>
    IDeclarationSelection<TTarget> WithTarget();
}