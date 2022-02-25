// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Validation;

/// <summary>
/// A delegate for validator methods. The <typeparamref name="TContext"/> type argument can be either <see cref="DeclarationValidationContext"/> or <see cref="ReferenceValidationContext"/>.
/// </summary>
/// <typeparam name="TContext">Either <see cref="DeclarationValidationContext"/> or <see cref="ReferenceValidationContext"/>.</typeparam>
/// <seealso href="@validation"/>
public delegate void ValidatorDelegate<TContext>( in TContext context );