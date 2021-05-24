// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Identifies interfaces that can be the target of aspects.
    /// </summary>
    /// <see cref="IAspect{T}"/>
    [CompileTimeOnly]
    public interface IAspectTarget : IDeclaration { }
}