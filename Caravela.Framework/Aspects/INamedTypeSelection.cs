// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents a set of types. Offers the ability to add aspects to types or to select members
    /// using <see cref="WithMethods"/>, <see cref="WithProperties"/>, <see cref="WithEvents"/>, <see cref="WithFields"/> or <see cref="WithConstructors"/>.
    /// </summary>
    public interface INamedTypeSelection : IDeclarationSelection<INamedType>
    {
        IDeclarationSelection<IMethod> WithMethods( Func<INamedType, IEnumerable<IMethod>> selector );

        IDeclarationSelection<IProperty> WithProperties( Func<INamedType, IEnumerable<IProperty>> selector );

        IDeclarationSelection<IEvent> WithEvents( Func<INamedType, IEnumerable<IEvent>> selector );

        IDeclarationSelection<IField> WithFields( Func<INamedType, IEnumerable<IField>> selector );

        IDeclarationSelection<IConstructor> WithConstructors( Func<INamedType, IEnumerable<IConstructor>> selector );
    }
}