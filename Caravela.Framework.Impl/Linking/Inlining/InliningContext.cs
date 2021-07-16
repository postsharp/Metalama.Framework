// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
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

        public IMethodSymbol TargetDeclaration { get; }

        public IMethodSymbol CurrentDeclaration { get; }

        private InliningContext( LinkerRewritingDriver rewritingDriver, IMethodSymbol targetDeclaration )
        {
            this._rewritingDriver = rewritingDriver;
            this.HasIndirectReturn = false;
            this.ReturnVariableName = null;
            this.ReturnLabelName = null;
            this._depth = 0;
            this.TargetDeclaration = targetDeclaration;
            this.CurrentDeclaration = targetDeclaration;
        }

        private InliningContext( InliningContext parent, IMethodSymbol currentDeclaration, string? returnVariableName, bool declaresReturnVariable = false )
        {
            this._rewritingDriver = parent._rewritingDriver;
            this.HasIndirectReturn = true;
            this.ReturnVariableName = returnVariableName;
            this._depth = parent._depth + 1;
            this.ReturnLabelName = $"__aspect_return_{this._depth}";
            this.DeclaresReturnVariable = declaresReturnVariable;
            this.TargetDeclaration = parent.TargetDeclaration;
            this.CurrentDeclaration = currentDeclaration;
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
                                                    LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( targetSymbol.ReturnType ),
                                                    SingletonSeparatedList( VariableDeclarator( this.ReturnVariableName.AssertNotNull() ) ) ) )
                                            .WithLeadingTrivia( ElasticLineFeed )
                                        : null,
                                    linkedBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    this._labelUsed
                                        ? LabeledStatement(
                                                Identifier( this.ReturnLabelName.AssertNotNull() ),
                                                EmptyStatement() )
                                            .WithLeadingTrivia( ElasticLineFeed )
                                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.EmptyLabeledStatement )
                                        : null
                                }.Where( x => x != null )
                                .AssertNoneNull() )
                        .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
            else
            {
                return linkedBody.AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
        }

        public static InliningContext Create( LinkerRewritingDriver rewritingDriver, IMethodSymbol targetDeclaration )
        {
            return new( rewritingDriver, targetDeclaration );
        }

        public InliningContext WithDeclaredReturnLocal( IMethodSymbol currentDeclaration )
        {
            return new( this, currentDeclaration, $"__aspect_return_{this._depth}", true );
        }

        public InliningContext WithReturnLocal( IMethodSymbol currentDeclaration, string valueText )
        {
            return new( this, currentDeclaration, valueText );
        }

        internal InliningContext WithDiscard( IMethodSymbol currentDeclaration )
        {
            return new( this, currentDeclaration, null );
        }

        public void UseLabel()
        {
            this._labelUsed = true;
        }
    }
}