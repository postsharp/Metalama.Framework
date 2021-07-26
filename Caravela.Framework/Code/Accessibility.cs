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
        // IMPORTANT: Don't change the order.
        Private,
        PrivateProtected,
        Protected,
        Internal,
        ProtectedInternal,
        Public
    }
}