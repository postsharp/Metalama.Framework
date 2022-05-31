// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating
{
    // ReSharper disable UnusedMember.Global

    /// <summary>
    /// This class is used at *run-time* by the generated template code. Do not remove or refactor
    /// without analysing impact on generated code.
    /// </summary>
    [Obfuscation( Exclude = true )]
    public static class TemplateSyntaxFactory
    {
        private static readonly SyntaxAnnotation _flattenBlockAnnotation = new( "Metalama_Flatten" );

        public static void AddStatement( List<StatementOrTrivia> list, StatementSyntax statement ) => list.Add( new StatementOrTrivia( statement, false ) );

        public static void AddStatement( List<StatementOrTrivia> list, IStatement statement )
            => list.Add( new StatementOrTrivia( ((UserStatement) statement).Syntax, true ) );

        public static void AddStatement( List<StatementOrTrivia> list, IExpression statement )
            => list.Add(
                new StatementOrTrivia(
                    SyntaxFactory.ExpressionStatement(
                        ((IUserExpression) statement).ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext ).Syntax ),
                    true ) );

        public static void AddStatement( List<StatementOrTrivia> list, string statement )
            => list.Add( new StatementOrTrivia( SyntaxFactory.ParseStatement( statement ), true ) );

        public static void AddComments( List<StatementOrTrivia> list, params string?[]? comments )
        {
            static IEnumerable<SyntaxTrivia> CreateTrivia( string comment )
            {
                if ( comment.Contains( '\n' ) || comment.Contains( '\r' ) )
                {
                    yield return SyntaxFactory.ElasticLineFeed;
                    yield return SyntaxFactory.Comment( "/* " + comment + " */" );
                }
                else
                {
                    yield return SyntaxFactory.ElasticLineFeed;
                    yield return SyntaxFactory.Comment( "// " + comment );
                }
            }

            if ( comments != null )
            {
                list.Add( new StatementOrTrivia( SyntaxFactory.TriviaList( comments.WhereNotNull().SelectMany( CreateTrivia ) ) ) );
            }
        }

        public static StatementSyntax? ToStatement( ExpressionSyntax expression )
        {
            if ( expression.Kind() == SyntaxKind.NullLiteralExpression )
            {
                // This is a special hack used when e.g. `meta.Proceed();` is invoked on a non-existing method.
                // Such call would be translated to the `null` expression, and we need to ignore it.
                return null;
            }
            else
            {
                return SyntaxFactory.ExpressionStatement( expression );
            }
        }

        public static SyntaxList<StatementSyntax> ToStatementList( List<StatementOrTrivia> list )
        {
            var statementList = new List<StatementSyntax>( list.Count );

            // List of trivia added by previous items in the list, and not yet added to a statement.
            // This is used when trivia are added before the first statement and after a first newline withing the trivia block.
            var nextLeadingTrivia = SyntaxTriviaList.Empty;

            foreach ( var statementOrTrivia in list )
            {
                switch ( statementOrTrivia.Statement )
                {
                    case StatementSyntax statement:
                        // Add
                        if ( nextLeadingTrivia.Count > 0 )
                        {
                            statement = statement.WithLeadingTrivia( nextLeadingTrivia.AddRange( statement.GetLeadingTrivia() ) );
                            nextLeadingTrivia = SyntaxTriviaList.Empty;
                        }

                        statementList.Add( statement );

                        break;

                    case SyntaxTriviaList trivia:
                        if ( statementList.Count == 0 )
                        {
                            // Add the trivia as the leading trivia of the next statement.
                            nextLeadingTrivia = nextLeadingTrivia.AddRange( trivia );
                        }
                        else
                        {
                            var previousStatement = statementList[statementList.Count - 1];

                            // TODO: Optimize the lookup for newline.
                            if ( previousStatement.GetTrailingTrivia().Any( x => x.IsKind( SyntaxKind.EndOfLineTrivia ) ) )
                            {
                                nextLeadingTrivia = nextLeadingTrivia.AddRange( trivia );
                            }
                            else
                            {
                                var triviaUpToFirstNewLine = SyntaxTriviaList.Empty;
                                var triviaAfterFirstNewLine = SyntaxTriviaList.Empty;
                                var isAfterFirstNewLine = false;

                                // Split the trivia after the first newline.
                                foreach ( var t in trivia )
                                {
                                    if ( !isAfterFirstNewLine )
                                    {
                                        triviaUpToFirstNewLine = triviaUpToFirstNewLine.Add( t );

                                        if ( t.IsKind( SyntaxKind.EndOfLineTrivia ) )
                                        {
                                            isAfterFirstNewLine = true;
                                        }
                                    }
                                    else
                                    {
                                        triviaAfterFirstNewLine = triviaAfterFirstNewLine.Add( t );
                                    }
                                }

                                statementList[statementList.Count - 1] =
                                    previousStatement
                                        .WithTrailingTrivia( previousStatement.GetTrailingTrivia().AddRange( triviaUpToFirstNewLine ) );

                                nextLeadingTrivia = triviaAfterFirstNewLine;
                            }
                        }

                        break;

                    default:
                        continue;
                }
            }

            // If there is any trivia left, we need to generate a dummy statement with the trivia (will be removed later).
            if ( nextLeadingTrivia.Count > 0 )
            {
                statementList.Add( SyntaxFactory.EmptyStatement().WithoutTrailingTrivia().WithLeadingTrivia( nextLeadingTrivia ) );
            }

            return SyntaxFactory.List( statementList );
        }

        public static BlockSyntax WithFlattenBlockAnnotation( this BlockSyntax block ) => block.WithAdditionalAnnotations( _flattenBlockAnnotation );

        public static bool HasFlattenBlockAnnotation( this BlockSyntax block ) => block.HasAnnotation( _flattenBlockAnnotation );

        public static SeparatedSyntaxList<T> SeparatedList<T>( params T[] items )
            where T : SyntaxNode
            => SyntaxFactory.SeparatedList( items );

        public static SyntaxKind Boolean( bool value ) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        // This method is called when the expression of 'return' is a non-dynamic expression.
        public static StatementSyntax ReturnStatement( ExpressionSyntax? returnExpression )
            => TemplateExpansionContext.Current.CreateReturnStatement( returnExpression, false );

        // This overload is called when the expression of 'return' is a compile-time expression returning a dynamic value.
        public static StatementSyntax DynamicReturnStatement( IUserExpression returnExpression, bool awaitResult )
            => TemplateExpansionContext.Current.CreateReturnStatement( returnExpression, awaitResult );

        public static StatementSyntax DynamicDiscardAssignment( IUserExpression? expression, bool awaitResult )
        {
            if ( expression == null )
            {
                return SyntaxFactoryEx.EmptyStatement;
            }
            else if ( TypeExtensions.Equals( expression.Type, SpecialType.Void ) )
            {
                return SyntaxFactory.ExpressionStatement( expression.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext ) );
            }
            else if ( awaitResult && TypeExtensions.Equals( expression.Type.GetAsyncInfo().ResultType, SpecialType.Void ) )
            {
                return
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression( expression.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext ) ) );
            }
            else
            {
                return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactoryEx.DiscardToken,
                        awaitResult
                            ? SyntaxFactory.AwaitExpression( expression.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext ) )
                            : expression.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext ) ) );
            }
        }

        // This overload is called when the value of a local is a dynamic expression but not a compile-time expression returning a dynamic value.
        public static StatementSyntax DynamicLocalDeclaration(
            TypeSyntax type,
            SyntaxToken identifier,
            IUserExpression? value,
            bool awaitResult )
        {
            if ( value == null )
            {
                // Don't know how to process this case. Find an example first.
                throw new AssertionFailedException();
            }

            var runtimeExpression = value.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext );

            if ( TypeExtensions.Equals( value.Type, SpecialType.Void )
                 || (awaitResult && TypeExtensions.Equals( value.Type.GetAsyncInfo().ResultType, SpecialType.Void )) )
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
                    return SyntaxFactory.Block(
                            awaitResult
                                ? SyntaxFactory.ExpressionStatement( SyntaxFactory.AwaitExpression( expressionStatement.Expression ) )
                                : expressionStatement,
                            localDeclarationStatement )
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
                                SyntaxFactory.EqualsValueClause(
                                    awaitResult
                                        ? SyntaxFactory.AwaitExpression( runtimeExpression )
                                        : runtimeExpression ) ) ) ) );
            }
        }

        public static RunTimeTemplateExpression? DynamicMemberAccessExpression( IUserExpression userExpression, string member )
        {
            if ( userExpression is UserReceiver dynamicMemberAccess )
            {
                return dynamicMemberAccess.CreateMemberAccessExpression( member );
            }

            var expression = userExpression.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext );

            return new RunTimeTemplateExpression(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression.Syntax,
                        SyntaxFactory.IdentifierName( member ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ),
                TemplateExpansionContext.Current.SyntaxGenerationContext );
        }

        public static SyntaxToken GetUniqueIdentifier( string hint )
            => SyntaxFactory.Identifier( TemplateExpansionContext.Current.LexicalScope.GetUniqueIdentifier( hint ) );

        public static ExpressionSyntax Serialize<T>( T o )
            => TemplateExpansionContext.Current.SyntaxSerializationService.Serialize(
                o,
                new SyntaxSerializationContext(
                    TemplateExpansionContext.Current.Compilation.AssertNotNull().GetCompilationModel(),
                    TemplateExpansionContext.CurrentSyntaxGenerationContext ) );

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
                case IUserExpression dynamicExpression:
                    return dynamicExpression.ToRunTimeTemplateExpression( TemplateExpansionContext.CurrentSyntaxGenerationContext );

                default:
                    if ( TemplateExpansionContext.Current.SyntaxSerializationService.TrySerialize(
                            expression,
                            TemplateExpansionContext.Current.SyntaxSerializationContext,
                            out var result ) )
                    {
                        return result;
                    }

                    throw new ArgumentOutOfRangeException( nameof(expression), $"Don't know how to extract the syntax from '{expression}'." );
            }
        }

        public static RunTimeTemplateExpression RuntimeExpression( ExpressionSyntax syntax, string? type = null )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var expressionType = type != null
                ? (ITypeSymbol?) DocumentationCommentId.GetFirstSymbolForDeclarationId( type, syntaxGenerationContext.Compilation )
                : null;

            return new RunTimeTemplateExpression( syntax, expressionType, syntaxGenerationContext, false );
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
            => TemplateExpansionContext.Current.SyntaxGenerationContext.ServiceProvider.GetRequiredService<CompileTimeTypeFactory>()
                .Get( new SymbolId( id ), name );

        public static TypeOfExpressionSyntax TypeOf( string typeId, Dictionary<string, TypeSyntax> substitutions )
        {
            var compilation = TemplateExpansionContext.Current.SyntaxGenerationContext.Compilation;
            var type = (ITypeSymbol?) new SymbolId( typeId ).Resolve( compilation );

            if ( type == null )
            {
                throw new InvalidOperationException( $"Cannot find the type {typeId} in compilation '{compilation.AssemblyName}'." );
            }

            return TemplateExpansionContext.Current.SyntaxGenerationContext.SyntaxGenerator.TypeOfExpression( type, substitutions );
        }
    }
}