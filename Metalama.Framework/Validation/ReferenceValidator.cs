// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Serialization;

namespace Metalama.Framework.Validation;

public abstract class Validator<T> : ICompileTimeSerializable
    where T : struct
{
    private protected Validator() { }

    public abstract void Validate( in T context );
}

public abstract class ReferenceValidator : Validator<ReferenceValidationContext>
{
    public virtual ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;
}