// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Advising;

/* The benefits of the design of having each advice kind return its own IAdviceResult interface are that:
    - it is possible to build fluent APIs based on advice results
    - it is possible to extend the interfaces with more properties 
*/

/// <summary>
/// A base interface that represents the result of any advice method of the <see cref="IAdviceFactory"/> interface.
/// </summary>
[CompileTime]
[InternalImplement]
public interface IAdviceResult
{
    AdviceKind AdviceKind { get; }

    /// <summary>
    /// Gets the advice outcome, i.e. indication whether the advice was applied, was ignored because the same declaration already exists (according to <see cref="OverrideStrategy"/>),
    /// or an error for different reasons. 
    /// </summary>
    AdviceOutcome Outcome { get; }

    /// <summary>
    /// Gets the <see cref="IAspectBuilder"/>.
    /// </summary>
    IAspectBuilder AspectBuilder { get; }
}

[CompileTime]
public enum AdviceKind
{
    None,
    OverrideMethod,
    OverrideFieldOrProperty,
    OverrideEvent,
    IntroduceMethod,
    IntroduceEvent,
    AddInitializer,
    AddAttribute,
    IntroduceParameter,
    AddContract,
    ImplementInterface,
    IntroduceField,
    IntroduceFinalizer,
    IntroduceOperator,
    IntroduceProperty,
    OverrideFinalizer,
    RemoveAttributes
}