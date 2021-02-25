﻿using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.UnitTests.Linker
{
    public static class SyntaxExtensions
    {
        public static SyntaxNode ToSyntaxNode( this ICodeElement codeElement )
        {
            if ( codeElement is CodeElement symbolicCodeElement )
            {
                return symbolicCodeElement.Symbol.DeclaringSyntaxReferences.Single().GetSyntax();
            }

            throw new NotImplementedException();
        }

        public static T ToSyntaxNode<T>( this ICodeElement codeElement )
            where T : SyntaxNode
        {
            return (T) ToSyntaxNode( codeElement );
        }

        public static string GetNormalizedText( this SyntaxTree syntaxTree )
        {
            return syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();
        }
    }
}
