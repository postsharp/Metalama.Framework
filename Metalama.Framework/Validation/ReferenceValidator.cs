// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Validation;

/// <summary>
/// A validator that can validate code references.
/// </summary>
public abstract class ReferenceValidator : Validator<ReferenceValidationContext>
{
    /// <summary>
    /// Gets the kinds of references for which the <see cref="Validator{TContext}.Validate"/> method should be invoked.
    /// </summary>
    public virtual ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;

    /// <summary>
    /// Gets a value indicating whether references to derived types should also be visited by the <see cref="Validator{TContext}.Validate"/> method.
    /// This property is only evaluated when the validated declaration is an <see cref="INamedType"/>.
    /// </summary>
    public virtual bool IncludeDerivedTypes => true;
}