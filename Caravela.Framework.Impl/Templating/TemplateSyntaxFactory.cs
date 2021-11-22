// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.SyntaxBuilders;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating
{
    // ReSharper disable UnusedMember.Global

    /// <summary>
    /// This class is used at *run-time* by the generated template code. Do not remove or refactor
    /// without analysing impact on generated code.
    /// </summary>
    [Obfuscation( Exclude = true )]
    public static class TemplateSyntaxFactory
    {
        private static readonly SyntaxAnnotation _flattenBlockAnnotation = new( "Caravela_Flatten" );

        public static void AddStatement( List<StatementOrTrivia> list, StatementSyntax statement ) => list.Add( new StatementOrTrivia( statement, false ) );

        public static void AddStatement( List<StatementOrTrivia> list, IStatement statement )
            => list.Add( new StatementOrTrivia( ((UserStatement) statement).Syntax, true ) );

        public static void AddStatement( List<StatementOrTrivia> list, IExpression statement )
            => list.Add(
                new StatementOrTrivia(
                    SyntaxFactory.ExpressionStatement( ((IUserExpression) statement).ToRunTimeExpression().Syntax ),
                    true ) );

        public static void AddStatement( List<StatementOrTrivia> list, string statement )
            => list.Add( new StatementOrTrivia( SyntaxFactory.ParseStatement( statement ), true ) );

        public static void AddComments( List<StatementOrTrivia> list, params string?[]? comments )
        {
            static SyntaxTrivia CreateTrivia( string comment )
            {
                if ( comment.Contains( '\n' ) || comment.Contains( '\r' ) )
                {
                    return SyntaxFactory.Comment( "/* " + comment + " */" + "\n" );
                }
                else
                {
                    return SyntaxFactory.Comment( "// " + comment + "\n" );
                }
            }

            if ( comments != null )
            {
                list.Add( new StatementOrTrivia( SyntaxFactory.TriviaList( comments.WhereNotNull().Select( CreateTrivia ) ) ) );
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
                switch ( statementOrTrivia.Statement )
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

        public static SeparatedSyntaxList<T> SeparatedList<T>( params T[] items )
            where T : SyntaxNode
            => SyntaxFactory.SeparatedList( items );

        public static SyntaxKind Boolean( bool value ) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        // This method is called when the expression of 'return' is a non-dynamic expression.
        public static StatementSyntax ReturnStatement( ExpressionSyntax? returnExpression )
            => TemplateExpansionContext.Current.CreateReturnStatement( returnExpression );

        // This overload is called when the expression of 'return' is a compile-time expression returning a dynamic value.
        public static StatementSyntax DynamicReturnStatement( IUserExpression returnExpression )
            => TemplateExpansionContext.Current.CreateReturnStatement( returnExpression );

        public static StatementSyntax DynamicDiscardAssignment( IUserExpression? expression )
        {
            if ( expression == null )
            {
                return SyntaxFactoryEx.EmptyStatement;
            }
            else if ( TypeExtensions.Equals( expression.Type, SpecialType.Void ) )
            {
                return SyntaxFactory.ExpressionStatement( expression.ToRunTimeExpression() );
            }
            else
            {
                return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactoryEx.DiscardToken,
                        expression.ToRunTimeExpression() ) );
            }
        }

        // This overload is called when the value of a local is a dynamic expression but not a compile-time expression returning a dynamic value.
        public static StatementSyntax DynamicLocalDeclaration(
            TypeSyntax type,
            SyntaxToken identifier,
            IUserExpression? value )
        {
            if ( value == null )
            {
                // Don't know how to process this case. Find an example first.
                throw new AssertionFailedException();
            }

            var runtimeExpression = value.ToRunTimeExpression();

            if ( TypeExtensions.Equals( value.Type, SpecialType.Void ) )
            {
                // If the method is void, we invoke the method as a statement (so we don't loose the side effect) and we define a local that
                // we assign to the default value. The local is necessary because it may be referenced later.
                TypeSyntax variableType;
                ExpressionSyntax variableValue;

                switch ( type )
                {
                    case IdentifierNameSyntax { IsVar: true }:
                        variableType = TemplateExpansionContext.CurrentSyntaxGenerationContext.SyntaxGenerator.Type(
                            Microsoft.CodeAnalysis.SpecialType.System_Object );

                        variableValue = SyntaxFactoryEx.Null;

                        break;

                    default:
                        variableType = type;
                        variableValue = SyntaxFactory.DefaultExpression( variableType );

                        break;
                }

                var expressionStatement = (ExpressionStatementSyntax?) runtimeExpression;

                var localDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            variableType,
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    identifier,
                                    null,
                                    SyntaxFactory.EqualsValueClause( variableValue ) ) ) ) )
                    .WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );

                if ( expressionStatement != null )
                {
                    return SyntaxFactory.Block( expressionStatement, localDeclarationStatement )
                        .WithFlattenBlockAnnotation();
                }
                else
                {
                    return localDeclarationStatement;
                }
            }
            else
            {
                return SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        type,
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                identifier,
                                null,
                                SyntaxFactory.EqualsValueClause( runtimeExpression ) ) ) ) );
            }
        }

        public static RuntimeExpression? DynamicMemberAccessExpression( IUserExpression userExpression, string member )
        {
            if ( userExpression is IUserReceiver dynamicMemberAccess )
            {
                return dynamicMemberAccess.CreateMemberAccessExpression( member );
            }

            var expression = userExpression.ToRunTimeExpression();

            return new RuntimeExpression(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression.Syntax,
                        SyntaxFactory.IdentifierName( member ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ),
                TemplateExpansionContext.Current.Compilation.AssertNotNull(),
                TemplateExpansionContext.Current.SyntaxGenerationContext.ServiceProvider );
        }

        public static SyntaxToken GetUniqueIdentifier( string hint )
            => SyntaxFactory.Identifier( TemplateExpansionContext.Current.LexicalScope.GetUniqueIdentifier( hint ) );

        public static ExpressionSyntax Serialize<T>( T o )
            => TemplateExpansionContext.Current.SyntaxSerializationService.Serialize(
                o,
                new SyntaxSerializationContext(
                    TemplateExpansionContext.Current.Compilation.AssertNotNull().GetCompilationModel(),
                    TemplateExpansionContext.CurrentSyntaxGenerationContext.SyntaxGenerator ) );

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

                            var appendedText = previousText.TextToken.ValueText + text.TextToken.ValueText;

                            var escapedTextWithQuotes =
                                SyntaxFactory.Literal( appendedText ).Text.Replace( "{", "{{" ).Replace( "}", "}}" );

                            var escapedText = escapedTextWithQuotes.Substring( 1, escapedTextWithQuotes.Length - 2 );

                            contents[previousIndex] = previousText.WithTextToken(
                                SyntaxFactory.Token( default, SyntaxKind.InterpolatedStringTextToken, escapedText, appendedText, default ) );
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

        public static IUserExpression? Proceed( string methodName ) => TemplateExpansionContext.Current.Proceed( methodName );

        public static ValueTask<object?> ProceedAsync() => meta.Proceed();

        public static ExpressionSyntax? GetDynamicSyntax( object? expression )
        {
            switch ( expression )
            {
                case null:
                    // This is typically because we are emitting the return value of a void method.
                    return null;

                case IUserExpression dynamicExpression:
                    return dynamicExpression.ToRunTimeExpression();

                default:
                    throw new ArgumentOutOfRangeException( nameof(expression), $"Don't know how to extract the syntax from '{expression}'." );
            }
        }

        public static RuntimeExpression RuntimeExpression( ExpressionSyntax syntax, string? type = null )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var expressionType = type != null
                ? (ITypeSymbol?) DocumentationCommentId.GetFirstSymbolForDeclarationId( type, syntaxGenerationContext.Compilation )
                : null;

            return new RuntimeExpression( syntax, expressionType, syntaxGenerationContext, false );
        }

        public static ExpressionSyntax SuppressNullableWarningExpression( ExpressionSyntax operand )
        {
            if ( TemplateExpansionContext.Current.SyntaxGenerator.IsNullAware )
            {
                return SyntaxFactory.PostfixUnaryExpression( SyntaxKind.SuppressNullableWarningExpression, operand );
            }
            else
            {
                return operand;
            }
        }

        public static ExpressionSyntax StringLiteralExpression( string? value ) => SyntaxFactoryEx.LiteralExpression( value );

        public static Type GetCompileTimeType( string id, string name )
            => TemplateExpansionContext.Current.SyntaxGenerationContext.ServiceProvider.GetService<CompileTimeTypeFactory>().Get( new SymbolId(id), name );
    }
}