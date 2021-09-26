// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents a set of types. Offers the ability to add aspects to types or to select members
    /// using <see cref="WithMembers{T}"/>.
    /// </summary>
    public interface INamedTypeSelection : IDeclarationSelection<INamedType>
    {
        IDeclarationSelection<T> WithMembers<T>( Func<INamedType, IEnumerable<T>> selector )
            where T : class, IDeclaration;
    }
}