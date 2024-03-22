// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Validation;

internal sealed record ValidationResult(
    bool HasDeclarationValidator,
    ImmutableArray<ReferenceValidatorInstance> ExternallyVisibleValidations,
    ImmutableUserDiagnosticList Diagnostics );