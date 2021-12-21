namespace Metalama.Framework.Validation;

/// <summary>
/// A delegate for validator methods. The <typeparamref name="TContext"/> type argument can be either <see cref="DeclarationValidationContext"/> or <see cref="ReferenceValidationContext"/>.
/// </summary>
/// <typeparam name="TContext">Either <see cref="DeclarationValidationContext"/> or <see cref="ReferenceValidationContext"/></typeparam>
public delegate void ValidatorDelegate<TContext>( in TContext context );