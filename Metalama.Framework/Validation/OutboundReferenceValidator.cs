// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Validation;

/// <summary>
/// A reference validator that validates outbound references. Its <see cref="BaseReferenceValidator.ValidateReferences"/> method
/// will be called for all declarations that are <i>referencing</i> the declaration to which this validator is added.
/// </summary>
public abstract class OutboundReferenceValidator : BaseReferenceValidator
{
    public sealed override ReferenceEndRole ValidatedEndRole => ReferenceEndRole.Origin;
}