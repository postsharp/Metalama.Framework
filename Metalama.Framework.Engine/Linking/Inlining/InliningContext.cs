// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Inlining
{
    internal sealed class InliningContext
    {
        private readonly LinkerRewritingDriver _rewritingDriver;
        private readonly int _depth;
        private bool _labelUsed;

        public bool HasIndirectReturn { get; }

        public string? ReturnVariableName { get; }

        public string? ReturnLabelName { get; }

        public bool DeclaresReturnVariable { get; }

        public IMethodSymbol TargetDeclaration { get; }

        public IMethodSymbol CurrentDeclaration { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        private InliningContext( LinkerRewritingDriver rewritingDriver, IMethodSymbol targetDeclaration, SyntaxGenerationContext syntaxGenerationContext )
        {
            this._rewritingDriver = rewritingDriver;
            this.HasIndirectReturn = false;
            this.ReturnVariableName = null;
            this.ReturnLabelName = null;
            this._depth = 0;
            this.TargetDeclaration = targetDeclaration;
            this.SyntaxGenerationContext = syntaxGenerationContext;
            this.CurrentDeclaration = targetDeclaration;
        }

        private InliningContext( InliningContext parent, IMethodSymbol currentDeclaration, string? returnVariableName, bool declaresReturnVariable = false )
        {
            this._rewritingDriver = parent._rewritingDriver;
            this.HasIndirectReturn = true;
            this.ReturnVariableName = returnVariableName;
            this.SyntaxGenerationContext = parent.SyntaxGenerationContext;
            this._depth = parent._depth + 1;
            this.ReturnLabelName = $"__aspect_return_{this._depth}";
            this.DeclaresReturnVariable = declaresReturnVariable;
            this.TargetDeclaration = parent.TargetDeclaration;
            this.CurrentDeclaration = currentDeclaration;
        }

        public BlockSyntax GetLinkedBody( IntermediateSymbolSemantic<IMethodSymbol> semantic )
        {
            var linkedBody = this._rewritingDriver.GetLinkedBody( semantic, this );

            if ( this.HasIndirectReturn )
            {
                return
                    Block(
                            new StatementSyntax?[]
                                {
                                    this.DeclaresReturnVariable
                                        ? LocalDeclarationStatement(
                                                VariableDeclaration(
                                                    this.SyntaxGenerationContext.SyntaxGenerator.Type( semantic.Symbol.ReturnType ),
                                                    SingletonSeparatedList( VariableDeclarator( this.ReturnVariableName.AssertNotNull() ) ) ) )
                                            .WithTrailingTrivia( ElasticLineFeed )
                                            .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation )
                                        : null,
                                    linkedBody.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock ),
                                    this._labelUsed
                                        ? LabeledStatement(
                                                Identifier( this.ReturnLabelName.AssertNotNull() ),
                                                EmptyStatement() )
                                            .WithTrailingTrivia( ElasticLineFeed )
                                            .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation )
                                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.EmptyLabeledStatement )
                                        : null
                                }.Where( x => x != null )
                                .AssertNoneNull() )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
            else
            {
                return linkedBody.WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
        }

        public static InliningContext Create(
            LinkerRewritingDriver rewritingDriver,
            IMethodSymbol targetDeclaration,
            SyntaxGenerationContext generationContext )
            => new( rewritingDriver, targetDeclaration, generationContext );

        [ExcludeFromCodeCoverage]
        public InliningContext WithDeclaredReturnLocal( IMethodSymbol currentDeclaration )
            => new( this, currentDeclaration, $"__aspect_return_{this._depth}", true );

        public InliningContext WithReturnLocal( IMethodSymbol currentDeclaration, string valueText ) => new( this, currentDeclaration, valueText );

        internal InliningContext WithDiscard( IMethodSymbol currentDeclaration ) => new( this, currentDeclaration, null );

        public void UseLabel() => this._labelUsed = true;
    }
}