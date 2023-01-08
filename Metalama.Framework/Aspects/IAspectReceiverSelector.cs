// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects;

/// <summary>
/// An interface that allows aspects and fabrics to register aspects and validators for current compilation version.
/// </summary>
public interface IAspectReceiverSelector<out TTarget> : IValidatorReceiverSelector<TTarget>
    where TTarget : class, IDeclaration
{
    /// <summary>
    /// Selects members of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>,
    /// <see cref="IValidatorReceiver{TDeclaration}.Validate"/>
    /// or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences"/>.
    /// </summary>
    new IAspectReceiver<TMember> With<TMember>( Func<TTarget, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects a member or the parent of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>.  <see cref="IValidatorReceiver{TDeclaration}.Validate"/>
    /// or <see cref="IValidatorReceiver{TDeclaration}.ValidateReferences"/>.
    /// </summary>
    new IAspectReceiver<TMember> With<TMember>( Func<TTarget, TMember> selector )
        where TMember : class, IDeclaration;
}