// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Advising;

/// <summary>
/// Enumerates the kinds of advice.
/// </summary>
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