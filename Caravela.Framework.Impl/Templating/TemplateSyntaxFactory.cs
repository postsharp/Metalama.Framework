// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// This class is used at *run-time* by the generated template code. Do not remove or refactor
    /// without analysing impact on generated code.
    /// </summary>
    [Obfuscation( Exclude = true )]
    public static class TemplateSyntaxFactory
    {
        private static readonly SyntaxAnnotation _flattenBlockAnnotation = new( "flatten" );

        private static readonly AsyncLocal<TemplateExpansionContext?> _expansionContext = new();

        internal static TemplateExpansionContext ExpansionContext
            => _expansionContext.Value ?? throw new InvalidOperationException( "ExpansionContext cannot be null." );

        internal static IDisposable WithContext( TemplateExpansionContext expansionContext )
        {
            _expansionContext.Value = expansionContext;

            return new InitializeCookie();
        }

        public static void AddStatement( List<StatementOrTrivia> list, StatementSyntax statement ) => list.Add( new StatementOrTrivia( statement ) );

        public static void AddComments( List<StatementOrTrivia> list, params string[]? comments )
        {
            if ( comments != null && comments.Length > 0 )
            {
                list.Add( new StatementOrTrivia( SyntaxFactory.TriviaList( comments.Select( c => SyntaxFactory.Comment( "// " + c + "\n" ) ) ) ) );
            }
        }

        public static StatementSyntax? ToStatement( ExpressionSyntax? expression )
            => expression == null ? null : SyntaxFactory.ExpressionStatement( expression );

        public static StatementSyntax[] ToStatementArray( List<StatementOrTrivia> list )
        {
            var statementList = new List<StatementSyntax>( list.Count );

            // List of trivia added by previous items in the list, and not yet added to a statement.
            // This is used when trivia are added before the first statement.
            var previousTrivia = SyntaxTriviaList.Empty;

            foreach ( var statementOrTrivia in list )
            {
                switch ( statementOrTrivia.Content )
                {
                    case StatementSyntax statement:
                        // Add
                        if ( previousTrivia.Count > 0 )
                        {
                            statement = statement.WithLeadingTrivia( previousTrivia );
                            previousTrivia = SyntaxTriviaList.Empty;
                        }

                        statementList.Add( statement );

                        break;

                    case SyntaxTriviaList trivia:
                        if ( statementList.Count == 0 )
                        {
                            // It will be added as the leading trivia of the next statement.
                            previousTrivia = previousTrivia.AddRange( trivia );
                        }
                        else
                        {
                            var previousStatement = statementList[statementList.Count - 1];

                            statementList[statementList.Count - 1] =
                                previousStatement.WithTrailingTrivia( previousStatement.GetTrailingTrivia().AddRange( trivia ) );
                        }

                        break;

                    default:
                        continue;
                }
            }

            // If there was no statement at all and we have comments, we need to generate a dummy statement with trivia.
            if ( previousTrivia.Count > 0 )
            {
                // This produce incorrectly indented code, but I didn't find a quick way to solve it.
                previousTrivia = previousTrivia.Insert( 0, SyntaxFactory.ElasticLineFeed );
                statementList.Add( SyntaxFactoryEx.EmptyStatement.WithTrailingTrivia( previousTrivia ) );
            }

            return statementList.ToArray();
        }

        public static BlockSyntax WithFlattenBlockAnnotation( this BlockSyntax block ) => block.WithAdditionalAnnotations( _flattenBlockAnnotation );

        public static bool HasFlattenBlockAnnotation( this BlockSyntax block ) => block.HasAnnotation( _flattenBlockAnnotation );

        // ReSharper disable once UnusedMember.Global
        public static SeparatedSyntaxList<T> SeparatedList<T>( params T[] items )
            where T : SyntaxNode
            => SyntaxFactory.SeparatedList( items );

        public static SyntaxKind BooleanKeyword( bool value ) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        public static StatementSyntax TemplateReturnStatement( ExpressionSyntax? returnExpression )
            => ExpansionContext.CreateReturnStatement( returnExpression );

        public static RuntimeExpression? CreateDynamicMemberAccessExpression( IDynamicExpression dynamicExpression, string member )
        {
            if ( dynamicExpression is IDynamicReceiver dynamicMemberAccess )
            {
                return dynamicMemberAccess.CreateMemberAccessExpression( member );
            }

            var expression = dynamicExpression.CreateExpression();

            if ( expression == null )
            {
                return null;
            }

            return new RuntimeExpression(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression.Syntax,
                        SyntaxFactory.IdentifierName( member ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ) );
        }

        public static SyntaxToken GetUniqueIdentifier( string hint ) => SyntaxFactory.Identifier( ExpansionContext.LexicalScope.GetUniqueIdentifier( hint ) );

        public static ExpressionSyntax Serialize<T>( T? o ) => ExpansionContext.SyntaxSerializationService.Serialize( o, ExpansionContext.SyntaxFactory );

        public static T AddSimplifierAnnotations<T>( T node )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( Simplifier.Annotation );

        public static ExpressionSyntax RenderInterpolatedString( InterpolatedStringExpressionSyntax interpolatedString )
        {
            List<InterpolatedStringContentSyntax> contents = new( interpolatedString.Contents.Count );

            foreach ( var content in interpolatedString.Contents )
            {
                switch ( content )
                {
                    case InterpolatedStringTextSyntax text:
                        var previousIndex = contents.Count - 1;

                        if ( contents.Count > 0 && contents[previousIndex] is InterpolatedStringTextSyntax previousText )
                        {
                            // If we have two adjacent text tokens, we need to merge them, otherwise reformatting will add a white space.

                            var appendedText = previousText.TextToken.Text + text.TextToken.Text;

                            contents[previousIndex] = previousText.WithTextToken(
                                SyntaxFactory.Token( default, SyntaxKind.InterpolatedStringTextToken, appendedText, appendedText, default ) );
                        }
                        else
                        {
                            contents.Add( text );
                        }

                        break;

                    case InterpolationSyntax interpolation:
                        contents.Add( interpolation );

                        break;
                }
            }

            return interpolatedString.WithContents( SyntaxFactory.List( contents ) );
        }

        public static ExpressionSyntax ConditionalExpression( ExpressionSyntax condition, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse )
        {
            // We try simplify the conditional expression when the result is known when the template is expanded.

            switch ( condition.Kind() )
            {
                case SyntaxKind.TrueLiteralExpression:
                    return whenTrue;

                case SyntaxKind.FalseLiteralExpression:
                    return whenFalse;

                default:
                    return SyntaxFactory.ConditionalExpression( condition, whenTrue, whenFalse );
            }
        }

        private class InitializeCookie : IDisposable
        {
            public void Dispose()
            {
                _expansionContext.Value = null;
            }
        }
    }
}