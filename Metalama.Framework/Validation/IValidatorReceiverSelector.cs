// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Validation;

/// <summary>
/// An interface that allows aspects and fabrics to register validators for the initial or final compilation version.
/// </summary>
[InternalImplement]
[CompileTime]
public interface IValidatorReceiverSelector<out TTarget>
    where TTarget : class, IDeclaration
{
    /// <summary>
    /// Selects members of the target declaration of the current aspect or fabric with the purpose of adding validators to them
    /// using e.g. <see cref="IValidatorReceiver{TDeclaration}.Validate"/> or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences(Metalama.Framework.Validation.ValidatorDelegate{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceKinds)"/> .
    /// </summary>
    IValidatorReceiver<TMember> With<TMember>( Func<TTarget, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    IValidatorReceiver<TMember> With<TMember>( Func<TTarget, TMember> selector )
        where TMember : class, IDeclaration;
}