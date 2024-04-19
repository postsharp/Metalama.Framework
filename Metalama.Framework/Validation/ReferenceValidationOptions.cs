// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Validation;

/// <summary>
/// Options for validators added by <see cref="IValidatorReceiver.ValidateOutboundReferences(System.Action{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceGranularity,Metalama.Framework.Validation.ReferenceKinds,Metalama.Framework.Validation.ReferenceValidationOptions)"/>
/// when supplying a delegate.
/// </summary>
[Flags]
[CompileTime]
public enum ReferenceValidationOptions
{
    Default = 0,

    /// <summary>
    /// Indicates that references to derived types should also be visited by the validation method.
    /// This value is only taken into when the validated declaration is an <see cref="INamedType"/>.
    /// Equivalent to <see cref="BaseReferenceValidator.IncludeDerivedTypes"/>.
    /// </summary>
    IncludeDerivedTypes = 1

    // An envisioned option is ExcludeMembers, which would cause references to type members not to be referenced.
}