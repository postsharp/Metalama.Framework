// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Accessibility of types and members, for instance <see cref="Private"/> or <see cref="Public"/>.
    /// </summary>
    [CompileTimeOnly]
    public enum Accessibility
    {
        // TODO: This uses IL values, shouldn't it correspond to C#?
        Private,
        ProtectedInternal,
        Protected,
        PrivateProtected,
        Internal,
        Public
    }
}