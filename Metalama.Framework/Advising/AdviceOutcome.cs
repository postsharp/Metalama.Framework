// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Advising;

/// <summary>
/// Status codes of the result of an advice. This enum is exposed on the <see cref="IAdviceResult.Outcome"/> property
/// of the <see cref="IAdviceResult"/> interface, which is returned by all methods of the <see cref="IAdviceFactory"/> interface.
/// </summary>
[CompileTime]
public enum AdviceOutcome
{
    /// <summary>
    /// The advice was successfully applied and there was no conflict.
    /// </summary>
    Default,

    /// <summary>
    /// There was a conflict and the advice was successfully applied and the new advice overrides the previous declaration.
    /// </summary>
    Override,

    /// <summary>
    /// There was a conflict and the advice was successfully applied and the new advice hides the previous declaration with the <c>new</c> keyword.
    /// </summary>
    New,

    /// <summary>
    /// There was a conflict and the advice was ignored.
    /// </summary>
    Ignore,

    [Obsolete]
    Ignored = Ignore,

    /// <summary>
    /// There was a conflict or another error. The advice was ignored and the whole aspect was disabled using <see cref="IAspectBuilder.SkipAspect"/>.
    /// </summary>
    Error
}