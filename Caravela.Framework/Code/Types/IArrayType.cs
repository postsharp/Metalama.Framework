// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Types
{
    /// <summary>
    /// Represents an array, e.g. <c>T[]</c>.
    /// </summary>
    public interface IArrayType : IType
    {
        /// <summary>
        /// Gets the element type, e.g. the <c>T</c> in <c>T[]</c>.
        /// </summary>
        IType ElementType { get; }

        /// <summary>
        /// Gets the array rank (1 for <c>T[]</c>, 2 for <c>T[,]</c>, ...).
        /// </summary>
        int Rank { get; }
    }
}