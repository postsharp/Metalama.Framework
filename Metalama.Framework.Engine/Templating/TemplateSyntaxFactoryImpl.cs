// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating
{
    internal partial class TemplateSyntaxFactoryImpl : ITemplateSyntaxFactory
    {
        private readonly SyntaxGenerationContext _syntaxGenerationContext;
        private readonly TemplateExpansionContext _templateExpansionContext;

        public TemplateSyntaxFactoryImpl( TemplateExpansionContext templateExpansionContext )
        {
            this._templateExpansionContext = templateExpansionContext;
            this._syntaxGenerationContext = templateExpansionContext.SyntaxGenerationContext;
        }

        public void AddStatement( List<StatementOrTrivia> list, StatementSyntax statement ) => list.Add( new StatementOrTrivia( statement, false ) );

        public void AddStatement( List<StatementOrTrivia> list, IStatement statement )
            => list.Add( new StatementOrTrivia( ((UserStatement) statement).Syntax, true ) );

        public void AddStatement( List<StatementOrTrivia> list, IExpression expression )
        {
            var statement = SyntaxFactory.ExpressionStatement(
                ((IUserExpression) expression).ToExpressionSyntax( this._syntaxGenerationContext ).RemoveParenthesis() );

            list.Add(
                new StatementOrTrivia(
                    statement,
                    true ) );
        }

        public void AddStatement( List<StatementOrTrivia> list, string statement )
            => list.Add( new StatementOrTrivia( SyntaxFactory.ParseStatement( statement ), true ) );

        public void AddComments( List<StatementOrTrivia> list, params string?[]? comments )
        {
            static IEnumerable<SyntaxTrivia> CreateTrivia( string comment )
            {
                if ( comment.ContainsOrdinal( '\n' ) || comment.ContainsOrdinal( '\r' ) )
                {
                    yield return SyntaxFactory.ElasticCarriageReturnLineFeed;
                    yield return SyntaxFactory.Comment( "/* " + comment + " */" );
                    yield return SyntaxFactory.ElasticCarriageReturnLineFeed;
                }
                else
                {
                    yield return SyntaxFactory.ElasticCarriageReturnLineFeed;
                    yield return SyntaxFactory.Comment( "// " + comment );
                    yield return SyntaxFactory.ElasticCarriageReturnLineFeed;
                }
            }

            if ( comments != null )
            {
                list.Add( new StatementOrTrivia( SyntaxFactory.TriviaList( comments.WhereNotNull().SelectMany( CreateTrivia ) ) ) );
            }
        }

        public StatementSyntax? ToStatement( ExpressionSyntax expression )
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

        public SyntaxList<StatementSyntax> ToStatementList( List<StatementOrTrivia> list )
        {
            var statementList = new List<StatementSyntax>( list.Count );

            // List of trivia added by previous items in the list, and not yet added to a statement.
            // This is used when trivia are added before the first statement and after a first newline withing the trivia block.
            var nextLeadingTrivia = SyntaxTriviaList.Empty;

            foreach ( var statementOrTrivia in list )
            {
                switch ( statementOrTrivia.Content )
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

        public SeparatedSyntaxList<T> SeparatedList<T>( params T[] items )
            where T : SyntaxNode
            => SyntaxFactory.SeparatedList( items );

        public SyntaxKind Boolean( bool value ) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        // This method is called when the expression of 'return' is a non-dynamic expression.
        public StatementSyntax ReturnStatement( ExpressionSyntax? returnExpression )
            => this._templateExpansionContext.CreateReturnStatement( returnExpression, false );

        // This overload is called when the expression of 'return' is a compile-time expression returning a dynamic value.
        public StatementSyntax DynamicReturnStatement( IUserExpression returnExpression, bool awaitResult )
            => this._templateExpansionContext.CreateReturnStatement( returnExpression, awaitResult );

        public StatementSyntax DynamicDiscardAssignment( IUserExpression? expression, bool awaitResult )
        {
            if ( expression == null )
            {
                return SyntaxFactoryEx.EmptyStatement;
            }
            else if ( expression.Type.Equals( SpecialType.Void ) )
            {
                return SyntaxFactory.ExpressionStatement( expression.ToExpressionSyntax( this._syntaxGenerationContext ).RemoveParenthesis() );
            }
            else if ( awaitResult && expression.Type.GetAsyncInfo().ResultType.Equals( SpecialType.Void ) )
            {
                return
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression( expression.ToExpressionSyntax( this._syntaxGenerationContext ) )
                            .RemoveParenthesis() );
            }
            else
            {
                return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactoryEx.DiscardToken,
                            awaitResult
                                ? SyntaxFactory.AwaitExpression( expression.ToExpressionSyntax( this._syntaxGenerationContext ).RemoveParenthesis() )
                                : expression.ToExpressionSyntax( this._syntaxGenerationContext ) )
                        .RemoveParenthesis() );
            }
        }

        // This overload is called when the value of a local is a dynamic expression but not a compile-time expression returning a dynamic value.
        public StatementSyntax DynamicLocalDeclaration(
            TypeSyntax type,
            SyntaxToken identifier,
            IUserExpression? value,
            bool awaitResult )
        {
            if ( value == null )
            {
                // Don't know how to process this case. Find an example first.
                throw new AssertionFailedException( "The expression should not be null." );
            }

            var runtimeExpression = value.ToExpressionSyntax( this._syntaxGenerationContext );

            if ( value.Type.Equals( SpecialType.Void )
                 || (awaitResult && value.Type.GetAsyncInfo().ResultType.Equals( SpecialType.Void )) )
            {
                // If the method is void, we invoke the method as a statement (so we don't loose the side effect) and we define a local that
                // we assign to the default value. The local is necessary because it may be referenced later.
                TypeSyntax variableType;
                ExpressionSyntax variableValue;

                switch ( type )
                {
                    case IdentifierNameSyntax { IsVar: true }:
                        variableType = this._syntaxGenerationContext.SyntaxGenerator.Type( Microsoft.CodeAnalysis.SpecialType.System_Object );

                        variableValue = SyntaxFactoryEx.Null;

                        break;

                    default:
                        variableType = type;
                        variableValue = SyntaxFactory.DefaultExpression( variableType );

                        break;
                }

                var localDeclarationStatement = SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            variableType,
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    identifier,
                                    null,
                                    SyntaxFactory.EqualsValueClause( variableValue ) ) ) ) )
                    .WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );

                return SyntaxFactory.Block(
                        awaitResult
                            ? SyntaxFactory.ExpressionStatement( SyntaxFactory.AwaitExpression( runtimeExpression.RemoveParenthesis() ) )
                            : SyntaxFactory.ExpressionStatement( runtimeExpression.RemoveParenthesis() ),
                        localDeclarationStatement )
                    .WithFlattenBlockAnnotation();
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

        public TypedExpressionSyntax? DynamicMemberAccessExpression( IUserExpression userExpression, string member )
        {
            if ( userExpression is UserReceiver dynamicMemberAccess )
            {
                return dynamicMemberAccess.CreateMemberAccessExpression( member );
            }

            var expression = userExpression.ToTypedExpressionSyntax( this._syntaxGenerationContext );

            return new TypedExpressionSyntaxImpl(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression.Syntax,
                        SyntaxFactory.IdentifierName( member ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ),
                this._templateExpansionContext.SyntaxGenerationContext );
        }

        public SyntaxToken GetUniqueIdentifier( string hint )
            => SyntaxFactory.Identifier( this._templateExpansionContext.LexicalScope.GetUniqueIdentifier( hint ) );

        public ExpressionSyntax Serialize<T>( T o )
            => this._templateExpansionContext.SyntaxSerializationService.Serialize(
                o,
                new SyntaxSerializationContext(
                    this._templateExpansionContext.Compilation.AssertNotNull().GetCompilationModel(),
                    this._syntaxGenerationContext ) );

        public T AddSimplifierAnnotations<T>( T node )
            where T : SyntaxNode
            => node.WithSimplifierAnnotation();

        public ExpressionSyntax RenderInterpolatedString( InterpolatedStringExpressionSyntax interpolatedString )
            => this._syntaxGenerationContext.SyntaxGenerator.RenderInterpolatedString( interpolatedString );

        public ExpressionSyntax ConditionalExpression( ExpressionSyntax condition, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse )
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

        public IUserExpression? Proceed( string methodName ) => this._templateExpansionContext.Proceed( methodName );

        public ValueTask<object?> ProceedAsync() => meta.Proceed();

        public ExpressionSyntax? GetDynamicSyntax( object? expression )
        {
            switch ( expression )
            {
                case IUserExpression dynamicExpression:
                    return dynamicExpression.ToTypedExpressionSyntax( this._syntaxGenerationContext );

                default:
                    if ( this._templateExpansionContext.SyntaxSerializationService.TrySerialize(
                            expression,
                            this._templateExpansionContext.SyntaxSerializationContext,
                            out var result ) )
                    {
                        return result;
                    }

                    throw new ArgumentOutOfRangeException( nameof(expression), $"Don't know how to extract the syntax from '{expression}'." );
            }
        }

        public TypedExpressionSyntax RuntimeExpression( ExpressionSyntax syntax, string? type = null )
        {
            var syntaxGenerationContext = this._syntaxGenerationContext;

            var expressionType = type != null
                ? (ITypeSymbol?) DocumentationCommentId.GetFirstSymbolForDeclarationId( type, syntaxGenerationContext.Compilation )
                : null;

            return new TypedExpressionSyntaxImpl( syntax, expressionType, syntaxGenerationContext, false );
        }

        public ExpressionSyntax SuppressNullableWarningExpression( ExpressionSyntax operand )
        {
            if ( this._templateExpansionContext.SyntaxGenerator.IsNullAware )
            {
                return SyntaxFactory.PostfixUnaryExpression( SyntaxKind.SuppressNullableWarningExpression, operand );
            }
            else
            {
                return operand;
            }
        }

        public ExpressionSyntax StringLiteralExpression( string? value ) => SyntaxFactoryEx.LiteralExpression( value );

        public Type GetCompileTimeType( string id, string name )
        {
            var context = this._templateExpansionContext;

            return context.CompilationContext.CompileTimeTypeFactory
                .Get( new SerializableTypeId( id ) );
        }

        public TypeOfExpressionSyntax TypeOf( string typeId, Dictionary<string, TypeSyntax>? substitutions )
        {
            var typeOfExpression = (TypeOfExpressionSyntax) SyntaxFactory.ParseExpression( typeId );

            if ( substitutions is { Count: > 0 } )
            {
                var rewriter = new SerializedTypeOfRewriter( substitutions );
                typeOfExpression = (TypeOfExpressionSyntax) rewriter.Visit( typeOfExpression )!;
            }

            return typeOfExpression;
        }

        public InterpolationSyntax FixInterpolationSyntax( InterpolationSyntax interpolation ) => InterpolationSyntaxHelper.Fix( interpolation );
    }
}