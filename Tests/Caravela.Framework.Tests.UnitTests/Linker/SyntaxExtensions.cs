// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public static class SyntaxExtensions
    {
        public static SyntaxNode ToSyntaxNode( this IDeclaration declaration )
        {
            if ( declaration is Declaration symbolicCodeElement )
            {
                return symbolicCodeElement.Symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            }

            throw new NotImplementedException();
        }

        public static T ToSyntaxNode<T>( this IDeclaration declaration )
            where T : SyntaxNode
        {
            return (T) ToSyntaxNode( declaration );
        }

        public static string GetNormalizedText( this SyntaxTree syntaxTree )
        {
            return syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();
        }
    }
}