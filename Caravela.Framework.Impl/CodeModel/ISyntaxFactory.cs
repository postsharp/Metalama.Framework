// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Exposes methods that generate syntax.
    /// </summary>
    internal interface ISyntaxFactory
    {
        /// <summary>
        /// Gets a fully-qualified <see cref="NameSyntax"/> for a given reflection <see cref="Type"/>.
        /// </summary>
        TypeSyntax GetTypeSyntax( Type type );

        ITypeSymbol GetTypeSymbol( Type type );
    }
}