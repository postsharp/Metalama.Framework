// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Validation;

public interface IReferenceValidatorProvider
{
    ReferenceIndexerOptions Options { get; }

    ImmutableArray<ReferenceValidatorInstance> GetValidators( ISymbol symbol );
}