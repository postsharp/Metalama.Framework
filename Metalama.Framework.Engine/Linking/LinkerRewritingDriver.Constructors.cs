﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        public IReadOnlyList<MemberDeclarationSyntax> RewriteConstructor(
            ConstructorDeclarationSyntax constructorDeclaration,
            IMethodSymbol symbol,
            SyntaxGenerationContext generationContext )
        {
            return new[] { (ConstructorDeclarationSyntax) new CodeMarkRewriter( this._codeTransformationRegistry ).Visit( constructorDeclaration ).AssertNotNull() };
        }

        private class CodeMarkRewriter : CSharpSyntaxRewriter
        {
            private readonly LinkerCodeTransformationRegistry _codeTransformationRegistry;

            public CodeMarkRewriter( LinkerCodeTransformationRegistry codeTransformationRegistry )
            {
                this._codeTransformationRegistry = codeTransformationRegistry;
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node != null 
                    && node.GetLinkerMarkedNodeId() is not null and string id 
                    && this._codeTransformationRegistry.CodeTransformations.TryGetValue( id, out var marks ) )
                {
                    switch ( node )
                    {
                        case BlockSyntax block:
                            return block.WithStatements(
                                block.Statements.Insert(
                                    0,
                                    Block( marks.Select( m => (StatementSyntax) m.Operand.AssertNotNull() ).ToArray() )
                                    .NormalizeWhitespace()
                                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ) ) );

                        default:
                            throw new AssertionFailedException();
                    }                    
                }
                else if (node?.ContainsAnnotations == true)
                {
                    return base.Visit( node );
                }
                else
                {
                    return node;
                }
            }
        }
    }
}
