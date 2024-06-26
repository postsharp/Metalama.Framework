// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Validation;

/// <summary>
/// A base class for validators.
/// </summary>
/// <typeparam name="TContext">Type of context i.e. <see cref="ReferenceValidationContext"/>.</typeparam>
[PublicAPI]
public abstract class Validator<TContext> : ICompileTimeSerializable
    where TContext : struct
{
    private protected Validator() { }

    /// <summary>
    /// Validates the current declaration or reference.
    /// </summary>
    /// <param name="context"></param>
    public abstract void Validate( in TContext context );
}