// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Aspects;

/// <summary>
/// An interface that allows aspects and fabrics to register aspects and validators for current compilation version.
/// </summary>
public interface IAspectReceiverSelector<out TTarget> : IValidatorReceiverSelector<TTarget>
    where TTarget : class, IDeclaration { }