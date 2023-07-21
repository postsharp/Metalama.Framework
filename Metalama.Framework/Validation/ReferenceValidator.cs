// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Validation;

/// <summary>
/// A validator that can validate code references.
/// </summary>
public abstract class ReferenceValidator : Validator<ReferenceValidationContext>
{
    public virtual ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;

    public virtual bool IncludeDerivedTypes => true;
}