// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;

namespace Metalama.Framework.Validation;

[Obsolete( "Use OutboundReferenceValidator." )]
public abstract class ReferenceValidator : OutboundReferenceValidator
{
    [PublicAPI]
    public abstract void Validate( in ReferenceValidationContext context );

    public sealed override void ValidateReferences( ReferenceValidationContext context ) => this.Validate( context );

    // The default value is for backward compatibility.
    public sealed override ReferenceGranularity Granularity => ReferenceGranularity.Member;
}