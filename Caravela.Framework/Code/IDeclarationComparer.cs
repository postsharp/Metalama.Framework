// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Extends <see cref="IEqualityComparer{T}"/> with the method <see cref="Is(Caravela.Framework.Code.IType,Caravela.Framework.Code.IType)"/>,
    /// which checks for type inheritance and not equality.
    /// </summary>
    [CompileTimeOnly]
    public interface IDeclarationComparer : IEqualityComparer<IType>, IEqualityComparer<IDeclaration>, IEqualityComparer<INamedType>
    {
        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the left <see cref="IType"/> is assignable to right <see cref="IType"/>.
        /// </summary>
        /// <returns></returns>
        bool Is( IType left, IType right );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the left <see cref="Type"/> is assignable to right <see cref="Type"/>.
        /// </summary>
        /// <returns></returns>
        bool Is( IType left, Type right );
    }
}