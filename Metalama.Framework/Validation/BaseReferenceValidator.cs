// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Validation;

public abstract class BaseReferenceValidator : ICompileTimeSerializable
{
    internal BaseReferenceValidator() { }

    public abstract void ValidateReferences( ReferenceValidationContext context );

    /// <summary>
    /// Gets the kinds of references for which the <see cref="Validator{TContext}.Validate"/> method should be invoked.
    /// </summary>
    public virtual ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;

    /// <summary>
    /// Gets a value indicating whether references to derived types should also be visited by the <see cref="Validator{TContext}.Validate"/> method.
    /// This property is only evaluated when the validated declaration is an <see cref="INamedType"/>.
    /// </summary>
    public virtual bool IncludeDerivedTypes => true;

    public abstract ReferenceGranularity Granularity { get; }

    public abstract ReferenceDirection Direction { get; }
}