// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Validation;

internal record ValidationResult( ImmutableArray<ReferenceValidatorInstance> TransitiveValidations, ImmutableUserDiagnosticList Diagnostics );