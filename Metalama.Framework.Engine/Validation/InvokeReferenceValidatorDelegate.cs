// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal delegate void InvokeReferenceValidatorDelegate( object instance, in ValidateReferenceContext context );