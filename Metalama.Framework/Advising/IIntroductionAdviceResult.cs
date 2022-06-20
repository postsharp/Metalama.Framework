// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Advising;

/// <summary>
/// Represents the result of the <c>Introduce*</c> methods of the <see cref="IAdviceFactory"/> interface.
/// </summary>
public interface IIntroductionAdviceResult<out T> : IAdviceResult
    where T : class, ICompilationElement
{
    /// <summary>
    /// Gets the introduced or overridden declaration.
    /// </summary>
    T Declaration { get; }
}