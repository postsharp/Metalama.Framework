// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public interface IDeclarationComparer : IEqualityComparer<IType>, IEqualityComparer<IDeclaration>
    {
        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the current type is assignable to another given type,
        /// given as an <see cref="IType"/>.
        /// </summary>
        /// <returns></returns>
        bool Is( IType left, IType right );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <returns></returns>
        bool Is( IType left, Type right );
    }
}