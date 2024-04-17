// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Validation;

/// <summary>
/// A validator that can validate code references.
/// </summary>
[Obsolete( "Use OutboundReferenceValidator." )]
public abstract class ReferenceValidator : OutboundReferenceValidator
{
    public abstract void Validate( in ReferenceValidationContext context );

    public override void ValidateReferences( ReferenceValidationContext context ) => this.Validate( context );

    // The default value is for backward compatibility.
    public override ReferenceGranularity Granularity => ReferenceGranularity.Member;
}