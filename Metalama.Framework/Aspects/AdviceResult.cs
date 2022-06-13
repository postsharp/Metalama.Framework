// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.Aspects;

/// <summary>
/// Represents the result of a method of <see cref="IAdviceFactory"/>.
/// </summary>
/// <typeparam name="T">The type of declaration returned by the advice method.</typeparam>
public readonly struct AdviceResult<T>
{
    /// <summary>
    /// Gets the declaration created or transformed by the advice method. For introduction advice methods, this is the introduced declaration when a new
    /// declaration is introduced, or the existing declaration when a declaration of the same name and signature already exists. For advice that modify a field,
    /// this is the property that now represents the field.
    /// </summary>
    public T Declaration { get; }

    /// <summary>
    /// Gets a value indicating whether the advice was successful. This is always <c>true</c> except for introduction advice when the <c>whenExists</c>
    /// parameter is set to <see cref="OverrideStrategy.Ignore"/> and there is already a declaration of the same name and signature than the one to introduce.
    /// </summary>
    public bool IsSuccessful { get; }

    internal AdviceResult( T declaration, bool isSuccessful = true )
    {
        this.Declaration = declaration;
        this.IsSuccessful = isSuccessful;
    }
}