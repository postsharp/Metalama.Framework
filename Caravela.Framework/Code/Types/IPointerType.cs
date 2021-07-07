// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code.Types
{
    /// <summary>
    /// Represents an unsafe pointer type.
    /// </summary>
    public interface IPointerType : IType
    {
        /// <summary>
        /// Gets the type pointed at, that is, <c>T</c> for <c>*T</c>.
        /// </summary>
        IType PointedAtType { get; }
    }
}