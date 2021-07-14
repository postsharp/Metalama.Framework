// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating
{
    // TODO: This is a temporary implementation of TemplateExpansionContext.

    internal class TemplateExpansionContext
    {
        public TemplateLexicalScope LexicalScope { get; }

        public MetaApi MetaApi { get; }

        public TemplateExpansionContext(
            object templateInstance,
            MetaApi metaApi,
            ICompilation compilation,
            TemplateLexicalScope lexicalScope,
            SyntaxSerializationService syntaxSerializationService,
            ICompilationElementFactory syntaxFactory )
        {
            this.TemplateInstance = templateInstance;
            this.MetaApi = metaApi;
            this.Compilation = compilation;
            this.SyntaxSerializationService = syntaxSerializationService;
            this.SyntaxFactory = syntaxFactory;
            this.LexicalScope = lexicalScope;
            Invariant.Assert( this.DiagnosticSink.DefaultScope != null );
            Invariant.Assert( this.DiagnosticSink.DefaultScope!.Equals( this.MetaApi.Declaration ) );
        }

        public object TemplateInstance { get; }

        public ICompilation Compilation { get; }

        public SyntaxSerializationService SyntaxSerializationService { get; }

        public ICompilationElementFactory SyntaxFactory { get; }

        public StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression )
        {
            if ( returnExpression == null )
            {
                return ReturnStatement();
            }

            if ( this.MetaApi.Method.ReturnType.Is( SpecialType.Void ) )
            {
                switch ( returnExpression )
                {
                    case InvocationExpressionSyntax invocationExpression:
                        // Do not use discard on invocations, because it may be void.
                        return
                            Block(
                                    ExpressionStatement( invocationExpression ),
                                    ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                                .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );

                    case null:
                    case LiteralExpressionSyntax:
                    case IdentifierNameSyntax:
                        // No need to call the expression  because we are guaranteed to have no side effect and we don't 
                        // care about the value.
                        return ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );

                    default:
                        // Anything else should use discard.
                        return
                            Block(
                                    ExpressionStatement(
                                        AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            IdentifierName(
                                                Identifier(
                                                    TriviaList(),
                                                    SyntaxKind.UnderscoreToken,
                                                    "_",
                                                    "_",
                                                    TriviaList() ) ),
                                            CastExpression(
                                                PredefinedType( Token( SyntaxKind.ObjectKeyword ) ),
                                                returnExpression ) ) ),
                                    ReturnStatement() )
                                .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
                }
            }

            var returnExpressionKind = returnExpression.Kind();

            if ( returnExpressionKind == SyntaxKind.DefaultLiteralExpression || returnExpressionKind == SyntaxKind.NullLiteralExpression )
            {
                return ReturnStatement( returnExpression );
            }

            // TODO: validate the returnExpression according to the method's return type.
            return ReturnStatement(
                Token( SyntaxKind.ReturnKeyword ).WithLeadingTrivia( Whitespace( " " ) ),
                CastExpression( ParseTypeName( this.MetaApi.Method.ReturnType.ToDisplayString() ), returnExpression ),
                Token( SyntaxKind.SemicolonToken ) );
        }

        public UserDiagnosticSink DiagnosticSink => this.MetaApi.Diagnostics;

        public StatementSyntax CreateReturnStatement( IDynamicExpression? returnExpression, string? expressionText = null, Location? location = null )
        {
            if ( returnExpression == null )
            {
                return ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );
            }
            else if ( returnExpression.ExpressionType.Is( SpecialType.Void ) )
            {
                if ( this.MetaApi.Method.ReturnType.Is( SpecialType.Void ) )
                {
                    return
                        Block(
                                ExpressionStatement( returnExpression.CreateExpression( expressionText, location ) ),
                                ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.Flattenable );
                }
                else
                {
                    // TODO: Emit error.
                    throw new AssertionFailedException();
                }
            }
            else
            {
                return this.CreateReturnStatement( returnExpression.CreateExpression( expressionText, location ) );
            }
        }
    }
}