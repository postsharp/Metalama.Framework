// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An object used by the delegated passed to <see cref="IAspectBuilder{TAspectTarget}.BuildLayer"/> method of the aspect to provide advice, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectLayerBuilder{T}"/> interface.
    /// </summary>
    [InternalImplement]
    [CompileTime]
    public interface IAspectLayerBuilder { }

    /// <summary>
    /// An object used by the delegated passed to <see cref="IAspectBuilder{TAspectTarget}.BuildLayer"/> method of the aspect to provide advice, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectLayerBuilder"/> interface.
    /// </summary>
    public interface IAspectLayerBuilder<out TAspectTarget> : IAspectLayerBuilder, IAspectReceiverSelector<TAspectTarget>
        where TAspectTarget : class, IDeclaration { }
}