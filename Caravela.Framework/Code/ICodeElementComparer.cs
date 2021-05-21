// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    public interface ICodeElementComparer : IEqualityComparer<IType>, IEqualityComparer<ICodeElement>, IEqualityComparer<INamedType>
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