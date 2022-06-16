// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Advising;

/// <summary>
/// Represents the result of the <c>Override</c> methods of the <see cref="IAdviceFactory"/> interface.
/// </summary>
public interface IOverrideAdviceResult<out T> : IAdviceResult
    where T : class, ICompilationElement
{
    /// <summary>
    /// Gets the declaration transformed by the advice method. For advice that modify a field,
    /// this is the property that now represents the field.
    /// </summary>
    T Declaration { get; }
}