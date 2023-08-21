// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class TemplateSyntaxFactoryImpl : ITemplateSyntaxFactory
    {
        private readonly TemplateExpansionContext _templateExpansionContext;
        private readonly SyntaxSerializationContext _syntaxSerializationContext;
        private readonly ObjectReaderFactory _objectReaderFactory;

        public TemplateSyntaxFactoryImpl( TemplateExpansionContext templateExpansionContext )
        {
            this._templateExpansionContext = templateExpansionContext;
            this._syntaxSerializationContext = templateExpansionContext.SyntaxSerializationContext;
            this._objectReaderFactory = templateExpansionContext.ServiceProvider.GetRequiredService<ObjectReaderFactory>();
        }

        public ICompilation Compilation => this._templateExpansionContext.Compilation.AssertNotNull();

        public void AddStatement( List<StatementOrTrivia> list, StatementSyntax? statement ) => list.Add( new StatementOrTrivia( statement ) );

        public void AddStatement( List<StatementOrTrivia> list, IStatement statement )
            => list.Add( new StatementOrTrivia( ((UserStatement) statement).Syntax ) );

        public void AddStatement( List<StatementOrTrivia> list, IExpression expression )
        {
            var statement = SyntaxFactory.ExpressionStatement( expression.ToExpressionSyntax( this._syntaxSerializationContext ).RemoveParenthesis() );

            list.Add( new StatementOrTrivia( statement ) );
        }

        public void AddStatement( List<StatementOrTrivia> list, string statement )
            => list.Add( new StatementOrTrivia( SyntaxFactoryEx.ParseStatementSafe( statement ) ) );

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
                            var previousStatement = statementList[^1];

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

                                statementList[^1] =
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

        public SyntaxKind Boolean( bool value ) => value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

        // This method is called when the expression of 'return' is a non-dynamic expression.
        public StatementSyntax ReturnStatement( ExpressionSyntax? returnExpression )
            => this._templateExpansionContext.CreateReturnStatement( returnExpression, false );

        // This overload is called when the expression of 'return' is a compile-time expression returning a dynamic value.
        public StatementSyntax DynamicReturnStatement( IUserExpression returnExpression, bool awaitResult )
            => this._templateExpansionContext.CreateReturnStatement( returnExpression, awaitResult );

        public StatementSyntax DynamicLocalAssignment( IdentifierNameSyntax identifier, SyntaxKind kind, IUserExpression? expression, bool awaitResult )
        {
            if ( expression == null )
            {
                return SyntaxFactoryEx.EmptyStatement;
            }
            else if ( expression.Type.Equals( SpecialType.Void ) )
            {
                if ( kind != SyntaxKind.SimpleAssignmentExpression )
                {
                    throw new InvalidOperationException( 
                        $"Templates using context-dependent compound assignments (e.g. 'x += meta.Proceed()') cannot be expanded when the right side " +
                        $"expression is of type 'void'. Use a simple assignment ('x = meta.Proceed') instead." );
                }

                return SyntaxFactory.ExpressionStatement( expression.ToExpressionSyntax( this._syntaxSerializationContext ).RemoveParenthesis() );
            }
            else if ( awaitResult && expression.Type.GetAsyncInfo().ResultType.Equals( SpecialType.Void ) )
            {
                return
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AwaitExpression( expression.ToExpressionSyntax( this._syntaxSerializationContext ) )
                            .RemoveParenthesis() );
            }
            else
            {
                return SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                            kind,
                            identifier,
                            awaitResult
                                ? SyntaxFactory.AwaitExpression( expression.ToExpressionSyntax( this._syntaxSerializationContext ).RemoveParenthesis() )
                                : expression.ToExpressionSyntax( this._syntaxSerializationContext ) )
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

            var runtimeExpression = value.ToExpressionSyntax( this._syntaxSerializationContext );

            if ( value.Type.Equals( SpecialType.Void )
                 || (awaitResult && value.Type.GetAsyncInfo().ResultType.Equals( SpecialType.Void )) )
            {
                // If the method is void, we invoke the method as a statement (so we don't lose the side effect) and we define a local that
                // we assign to the default value. The local is necessary because it may be referenced later.
                TypeSyntax variableType;
                ExpressionSyntax variableValue;

                switch ( type )
                {
                    case IdentifierNameSyntax { IsVar: true }:
                        variableType = this._syntaxSerializationContext.SyntaxGenerator.Type( Microsoft.CodeAnalysis.SpecialType.System_Object );

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
                    .WithAdditionalAnnotations( FormattingAnnotations.PossibleRedundantAnnotation );

                // Special case for default(void), which cannot be an expression statement.
                if ( value is DefaultUserExpression )
                {
                    return localDeclarationStatement;
                }

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

        public TypedExpressionSyntax DynamicMemberAccessExpression( IUserExpression userExpression, string member )
        {
            if ( userExpression is UserReceiver dynamicMemberAccess )
            {
                return dynamicMemberAccess.CreateMemberAccessExpression( member );
            }

            var expression = userExpression.ToExpressionSyntax( this._syntaxSerializationContext );

            return new TypedExpressionSyntaxImpl(
                SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expression,
                        SyntaxFactory.IdentifierName( member ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ),
                this._syntaxSerializationContext.SyntaxGenerationContext );
        }

        public SyntaxToken GetUniqueIdentifier( string hint )
            => SyntaxFactory.Identifier( this._templateExpansionContext.LexicalScope.GetUniqueIdentifier( hint ) );

        public ExpressionSyntax Serialize<T>( T o )
            => this._templateExpansionContext.SyntaxSerializationService.Serialize( o, this._syntaxSerializationContext );

        public T AddSimplifierAnnotations<T>( T node )
            where T : SyntaxNode
            => node.WithSimplifierAnnotation();

        public ExpressionSyntax RenderInterpolatedString( InterpolatedStringExpressionSyntax interpolatedString )
            => this._syntaxSerializationContext.SyntaxGenerator.RenderInterpolatedString( interpolatedString );

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

        public IUserExpression Proceed( string methodName ) => this._templateExpansionContext.Proceed( methodName );

        public IUserExpression ConfigureAwait( IUserExpression expression, bool continueOnCapturedContext )
            => TemplateExpansionContext.ConfigureAwait( expression, continueOnCapturedContext );

        public ExpressionSyntax GetDynamicSyntax( object? expression )
        {
            switch ( expression )
            {
                case IExpression dynamicExpression:
                    return dynamicExpression.ToTypedExpressionSyntax( this._syntaxSerializationContext );

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

        public TypedExpressionSyntax GetTypedExpression( IExpression expression ) => expression.ToTypedExpressionSyntax( this._syntaxSerializationContext );

        public TypedExpressionSyntax RunTimeExpression( ExpressionSyntax syntax, string? type = null )
        {
            var syntaxSerializationContext = this._syntaxSerializationContext;

            var expressionType = type != null
                ? syntaxSerializationContext.CompilationContext.SerializableTypeIdResolver.ResolveId(
                    new SerializableTypeId( type ),
                    this._templateExpansionContext.TemplateGenericArguments )
                : null;

            return new TypedExpressionSyntaxImpl( syntax, expressionType, syntaxSerializationContext.SyntaxGenerationContext );
        }

        public IUserExpression GetUserExpression( object expression ) => ((IExpression) expression).ToUserExpression();

        public ExpressionSyntax SuppressNullableWarningExpression( ExpressionSyntax operand )
        {
            var suppressNullableWarning = false;

            if ( this._templateExpansionContext.SyntaxGenerator.IsNullAware )
            {
                suppressNullableWarning = true;

                if ( SymbolAnnotationMapper.TryFindExpressionTypeFromAnnotation(
                        operand,
                        this._syntaxSerializationContext.CompilationContext,
                        out var typeSymbol ) )
                {
                    // Value types, including nullable value types don't need suppression.
                    if ( typeSymbol is { IsValueType: true } )
                    {
                        suppressNullableWarning = false;
                    }

                    if ( typeSymbol?.IsNullable() == false )
                    {
                        suppressNullableWarning = false;
                    }
                }
            }

            return suppressNullableWarning
                ? SyntaxFactory.PostfixUnaryExpression( SyntaxKind.SuppressNullableWarningExpression, operand )
                : operand;
        }

        public ExpressionSyntax ConditionalAccessExpression( ExpressionSyntax expression, ExpressionSyntax whenNotNullExpression )
        {
            SymbolAnnotationMapper.TryFindExpressionTypeFromAnnotation( expression, this._syntaxSerializationContext.CompilationContext, out var typeSymbol );

            return typeSymbol?.IsNullable() != false
                ? SyntaxFactory.ConditionalAccessExpression( expression, whenNotNullExpression )
                : (ExpressionSyntax) new RemoveConditionalAccessRewriter( expression ).Visit( whenNotNullExpression )!;
        }

        public ExpressionSyntax StringLiteralExpression( string? value ) => SyntaxFactoryEx.LiteralExpression( value );

        public TypeOfExpressionSyntax TypeOf( string typeOfString, Dictionary<string, TypeSyntax>? substitutions )
        {
            var typeOfExpression = (TypeOfExpressionSyntax) SyntaxFactoryEx.ParseExpressionSafe( typeOfString );

            if ( substitutions is { Count: > 0 } )
            {
                var rewriter = new SerializedTypeOfRewriter( substitutions );
                typeOfExpression = (TypeOfExpressionSyntax) rewriter.Visit( typeOfExpression )!;
            }

            return typeOfExpression;
        }

        public InterpolationSyntax FixInterpolationSyntax( InterpolationSyntax interpolation ) => InterpolationSyntaxHelper.Fix( interpolation );

        public ITemplateSyntaxFactory ForLocalFunction( string returnType, Dictionary<string, IType> genericArguments, bool isAsync = false )
        {
            var returnTypeSymbol = new SerializableTypeId( returnType ).Resolve( this._templateExpansionContext.Compilation.AssertNotNull(), genericArguments );

            return new TemplateSyntaxFactoryImpl( this._templateExpansionContext.ForLocalFunction( new LocalFunctionInfo( returnTypeSymbol, isAsync ) ) );
        }

        private BlockSyntax? InvokeTemplate( string templateName, object? templateProvider, IObjectReader arguments )
        {
            var (templateClass, templateMember) = this.GetTemplateDescription( templateName, templateProvider );

            var context = this._templateExpansionContext.ForTemplate( templateMember, templateProvider );
            var templateArguments = templateMember.ArgumentsForCalledTemplate( arguments );

            // Add the first template argument.
            var allArguments = new object?[templateArguments.Length + 1];
            allArguments[0] = this.ForTemplate( templateName, templateProvider );
            templateArguments.CopyTo( allArguments, 1 );

            var compiledTemplateMethodInfo = templateClass.GetCompiledTemplateMethodInfo( templateMember.Declaration.GetSymbol().AssertNotNull() );
            return compiledTemplateMethodInfo.Invoke( context.TemplateInstance, allArguments ).AssertNotNull().AssertCast<BlockSyntax>();
        }

        public BlockSyntax? InvokeTemplate( string templateName, object? templateProvider = null, object? arguments = null )
        {
            return this.InvokeTemplate( templateName, templateProvider, this._objectReaderFactory.GetReader( arguments ) );
        }

        public BlockSyntax? InvokeTemplate( TemplateInvocation templateInvocation, object? arguments = null )
        {
            var invocationArgs = this._objectReaderFactory.GetReader( templateInvocation.Arguments );
            var directArgs = this._objectReaderFactory.GetReader( arguments );

            return this.InvokeTemplate( templateInvocation.TemplateName, templateInvocation.TemplateProvider, ObjectReader.Merge( invocationArgs, directArgs ) );
        }

        private (TemplateClass TemplateClass, TemplateMember<IMethod> TemplateMember) GetTemplateDescription( string templateName, object? templateProvider )
        {
            if ( templateProvider == this._templateExpansionContext.TemplateInstance )
            {
                templateProvider = null;
            }

            var templateClass = this._templateExpansionContext.GetTemplateClass( templateProvider );
            var template = AdviceFactory.ValidateTemplateName( templateClass, templateName, required: true )!;

            // TODO: do I need TMR? if I do, move it back to ValidateTemplateName? if I don't, extract relevant part from TMR.GetTemplateMember()
            var templateMemberRef = new TemplateMemberRef( template, TemplateKind.Default );
            var templateMember = templateMemberRef.GetTemplateMember<IMethod>( this.Compilation.GetCompilationModel(), this._templateExpansionContext.ServiceProvider );

            return (templateClass, templateMember);
        }

        public ITemplateSyntaxFactory ForTemplate( string templateName, object? templateProvider )
        {
            var templateMember = this.GetTemplateDescription( templateName, templateProvider ).TemplateMember;

            var context = this._templateExpansionContext.ForTemplate( templateMember, templateProvider );

            return context.SyntaxFactory;
        }

        public TemplateTypeArgument TemplateTypeArgument( string name, Type type )
            => TemplateBindingHelper.CreateTemplateTypeArgument( name, TypeFactory.Implementation.GetTypeByReflectionType( type ) );
    }
}