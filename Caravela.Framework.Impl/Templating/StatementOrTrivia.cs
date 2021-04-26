// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    [Obfuscation( Exclude = true )]
    public readonly struct StatementOrTrivia
    {
        public object? Content { get; }

        internal StatementOrTrivia( StatementSyntax content )
        {
            this.Content = content;
        }

        internal StatementOrTrivia( SyntaxTriviaList content )
        {
            this.Content = content;
        }
    }
}