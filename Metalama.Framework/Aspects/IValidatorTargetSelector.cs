// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects;

/// <summary>
/// An interface that allows aspects and fabrics to register validators for the initial or final compilation version.
/// </summary>
[InternalImplement]
public interface IValidatorTargetSelector<out TTarget>
    where TTarget : class, IDeclaration
{
    /// <summary>
    /// Selects members of the target declaration of the current aspect or fabric with the purpose of adding validators to them
    /// using e.g. <see cref="IValidatorReceiver{TDeclaration}.Validate"/> or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences"/> .
    /// </summary>
    IValidatorReceiver<TMember> WithTargetMembers<TMember>( Func<TTarget, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects the  target declaration of the current aspect or fabric  with the purpose of adding validators to them
    /// using e.g. <see cref="IValidatorReceiver{TDeclaration}.Validate"/> or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences"/> .
    /// </summary>
    IValidatorReceiver<TTarget> WithTarget();
}