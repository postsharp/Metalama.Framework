// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Validation;

/// <summary>
/// Abstract base class for validator of code references.
/// </summary>
public abstract class BaseReferenceValidator : ICompileTimeSerializable
{
    internal BaseReferenceValidator() { }

    /// <summary>
    /// Validates the references in the given context. The context may contain several references, all grouped according
    /// to the validator <see cref="Granularity"/>. The implementation is reponsible for reporting diagnostics as necessary.
    /// </summary>
    /// <param name="context"></param>
    public abstract void ValidateReferences( ReferenceValidationContext context );

    /// <summary>
    /// Gets the kinds of references for which the <see cref="ValidateReferences"/> method should be invoked.
    /// </summary>
    public virtual ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;

    /// <summary>
    /// Gets a value indicating whether references to derived types should also be visited by the <see cref="ValidateReferences"/> method.
    /// This property is only evaluated when the validated declaration is an <see cref="INamedType"/>.
    /// </summary>
    public virtual bool IncludeDerivedTypes => true;

    /// <summary>
    /// Gets level of declarations at which the analysis should be performed. For instance, if the <see cref="ValidateReferences"/> method
    /// only depends on the namespace of the referencing syntax (as opposed to its declaring member or type), then <see cref="Granularity"/> should be set
    /// to <see cref="ReferenceGranularity.Namespace"/>, and <see cref="ValidateReferences"/> will be invoked a single time per namespace.
    /// </summary>
    public abstract ReferenceGranularity Granularity { get; }

    /// <summary>
    /// Gets the direction (<see cref="ReferenceDirection.Inbound"/> or <see cref="ReferenceDirection.Outbound"/>) of validated reference ends.
    /// </summary>
    public abstract ReferenceDirection Direction { get; }
}