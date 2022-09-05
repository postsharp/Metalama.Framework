// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution
{
    /// <summary>
    /// Substitutes a node using an inliner.
    /// </summary>
    internal class InliningSubstitution : SyntaxNodeSubstitution
    {
        private readonly InliningSpecification _specification;

        public override SyntaxNode TargetNode => this._specification.ReplacedRootNode;

        public InliningSubstitution(InliningSpecification specification )
        {
            this._specification = specification;
        }

        public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext context )
        {
            var statements = new List<StatementSyntax>();

            if (this._specification.DeclareReturnVariable)
            {
                statements.Add(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            context.SyntaxGenerationContext.SyntaxGenerator.Type( GetReturnType( this._specification.AspectReference.OriginalSymbol ) ),
                            SingletonSeparatedList(
                                VariableDeclarator( this._specification.ReturnVariableIdentifier.AssertNotNull() ) ) ) )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation ) );
            }

            // Get substituted body of the target.
            var substitutedBody = context.RewritingDriver.GetSubstitutedBody( this._specification.TargetSemantic, context.WithInliningContext(this._specification.ContextIdentifier) );

            // Let the inliner to transform that.
            var inlinedBody = this._specification.Inliner.Inline( context.SyntaxGenerationContext, this._specification, currentNode, substitutedBody );

            statements.Add( inlinedBody );

            if ( this._specification.ReturnLabelIdentifier != null )
            {
                statements.Add(
                    LabeledStatement(
                        Identifier( this._specification.ReturnLabelIdentifier.AssertNotNull() ),
                        EmptyStatement() )
                    .WithTrailingTrivia( ElasticLineFeed )
                    .WithGeneratedCodeAnnotation( FormattingAnnotations.SystemGeneratedCodeAnnotation )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.EmptyLabeledStatement ) );
            }

            return Block( statements )
                .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
        }

        static ITypeSymbol GetReturnType(ISymbol symbol)
        {
            switch ( symbol )
            {
                case IMethodSymbol method:
                    return method.ReturnType;
                case IPropertySymbol property:
                    return property.Type;
                case IEventSymbol @event:
                    return @event.Type;
                default:
                    throw new AssertionFailedException();
            }
        }
    }
}