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
        // IMPORTANT: Don't change. Comparisons depend on the order.        
        // Reserve 0 is we ever need something like undefined values (see Roslyn).
        Private = 1,
        PrivateProtected = 2,
        Protected = 3,
        Internal = 4,
        ProtectedInternal = 5,
        Public = 6
    }
}