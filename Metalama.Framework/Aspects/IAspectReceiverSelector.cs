// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects;

/// <summary>
/// An interface that allows aspects and fabrics to register validators for current compilation version.
/// </summary>
public interface IAspectReceiverSelector<out TTarget> : IValidatorReceiverSelector<TTarget>
    where TTarget : class, IDeclaration
{
    /// <summary>
    /// Selects members of the  target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspect{TAspect}(System.Func{TDeclaration,System.Linq.Expressions.Expression{System.Func{TAspect}}})"/>,
    ///  <see cref="IAspectReceiver{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>,  <see cref="IValidatorReceiver{TDeclaration}.Validate"/>
    /// or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences"/>.
    /// </summary>
    new IAspectReceiver<TMember> WithTargetMembers<TMember>( Func<TTarget, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects the  target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspect{TAspect}(System.Func{TDeclaration,System.Linq.Expressions.Expression{System.Func{TAspect}}})"/>,
    ///  <see cref="IAspectReceiver{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>,  <see cref="IValidatorReceiver{TDeclaration}.Validate"/>
    /// or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences"/>.
    /// </summary>
    new IAspectReceiver<TTarget> WithTarget();

    /// <summary>
    /// Gets an interface that allows to validate the final compilation, after all aspects have been applied.
    /// </summary>
    IValidatorReceiverSelector<TTarget> AfterAllAspects();

    /// <summary>
    /// Gets an interface that allows to validate the initial compilation, after before any aspect has been applied.
    /// </summary>
    IValidatorReceiverSelector<TTarget> BeforeAnyAspect();
}