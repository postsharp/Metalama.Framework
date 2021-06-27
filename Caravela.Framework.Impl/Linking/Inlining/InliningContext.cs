// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal sealed class InliningContext
    {
        private readonly LinkerRewritingDriver _rewritingDriver;
        private readonly int _depth;
        private bool _labelUsed;

        public Compilation Compilation => this._rewritingDriver.IntermediateCompilation;

        public AspectReferenceResolver ReferenceResolver => this._rewritingDriver.ReferenceResolver;

        public bool HasIndirectReturn { get; }

        public string? ReturnVariableName { get; }

        public string? ReturnLabelName { get; }

        public bool DeclaresReturnVariable { get; }

        private InliningContext( LinkerRewritingDriver rewritingDriver )
        {
            this._rewritingDriver = rewritingDriver;
            this.HasIndirectReturn = false;
            this.ReturnVariableName = null;
            this.ReturnLabelName = null;
            this._depth = 0;
        }

        private InliningContext( InliningContext parent, string? returnVariableName, bool declaresReturnVariable = false )
        {
            this._rewritingDriver = parent._rewritingDriver;
            this.HasIndirectReturn = true;
            this.ReturnVariableName = returnVariableName;
            this._depth = parent._depth + 1;
            this.ReturnLabelName = $"__aspect_return_{this._depth}";
            this.DeclaresReturnVariable = declaresReturnVariable;
        }

        public BlockSyntax GetLinkedBody( IMethodSymbol targetSymbol )
        {
            var linkedBody = this._rewritingDriver.GetLinkedBody( targetSymbol, this );

            if ( this.HasIndirectReturn )
            {
                return
                    Block(
                        new StatementSyntax?[]
                        {
                            this.DeclaresReturnVariable
                            ? LocalDeclarationStatement(
                                VariableDeclaration( 
                                    (TypeSyntax)LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(targetSymbol.ReturnType), 
                                    SingletonSeparatedList( VariableDeclarator( this.ReturnVariableName.AssertNotNull() ) ) ) )
                            : null,
                            linkedBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable ),
                            this._labelUsed 
                            ? LabeledStatement(
                                Identifier( this.ReturnLabelName.AssertNotNull() ),
                                ExpressionStatement(
                                    IdentifierName( MissingToken( SyntaxKind.IdentifierToken ) ) )
                                .WithSemicolonToken( MissingToken( SyntaxKind.SemicolonToken ) ) )
                            : null
                        }.Where( x => x != null ).AssertNoneNull() )
                    .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
            }
            else
            {
                return linkedBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
            }
        }

        public static InliningContext Create( LinkerRewritingDriver rewritingDriver )
        {
            return new InliningContext( rewritingDriver );
        }

        public InliningContext WithDeclaredReturnLocal()
        {
            return new InliningContext( this, $"__aspect_return_{this._depth}", true );
        }

        public InliningContext WithReturnLocal( string valueText )
        {
            return new InliningContext( this, valueText );
        }

        internal InliningContext WithDiscard()
        {
            return new InliningContext( this, null );
        }

        public void UseLabel()
        {
            this._labelUsed = true;
        }
    }
}
