﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateExpansionContext
    {
        private readonly TemplateMember<IMethod> _templateMethod;
        private readonly IUserExpression? _proceedExpression;
        private static readonly AsyncLocal<TemplateExpansionContext?> _current = new();
        private static readonly AsyncLocal<SyntaxGenerationContext?> _currentSyntaxGenerationContext = new();

        internal static TemplateExpansionContext Current
            => _current.Value ?? throw new InvalidOperationException( "TemplateExpansionContext.Current has not be set." );

        /// <summary>
        /// Gets the current <see cref="SyntaxGenerationContext"/>.
        /// </summary>
        internal static SyntaxGenerationContext CurrentSyntaxGenerationContext
            => _current.Value?.SyntaxGenerationContext
               ?? _currentSyntaxGenerationContext.Value
               ?? throw new InvalidOperationException( "TemplateExpansionContext.CurrentSyntaxGenerationContext has not be set." );

        internal static IDisposable WithTemplateExpansionContext( TemplateExpansionContext expansionContext )
        {
            _current.Value = expansionContext;

            return new DisposeCookie( () => _current.Value = null );
        }

        /// <summary>
        /// Sets the <see cref="CurrentSyntaxGenerationContext"/> but not the <see cref="Current"/> property.
        /// This method is used in tests, when the <see cref="CurrentSyntaxGenerationContext"/> property is needed but not the <see cref="Current"/>
        /// one.
        /// </summary>
        internal static IDisposable WithSyntaxGenerationContext( SyntaxGenerationContext generationContext )
        {
            _currentSyntaxGenerationContext.Value = generationContext;

            return new DisposeCookie( () => _currentSyntaxGenerationContext.Value = null );
        }

        public TemplateLexicalScope LexicalScope { get; }

        public MetaApi MetaApi { get; }

        public TemplateExpansionContext(
            object templateInstance,
            MetaApi metaApi,
            ICompilation compilation,
            TemplateLexicalScope lexicalScope,
            SyntaxSerializationService syntaxSerializationService,
            SyntaxGenerationContext syntaxGenerationContext,
            TemplateMember<IMethod> templateMethod,
            IUserExpression? proceedExpression )
        {
            this._templateMethod = templateMethod;
            this.TemplateInstance = templateInstance;
            this.MetaApi = metaApi;
            this.Compilation = compilation;
            this.SyntaxSerializationService = syntaxSerializationService;
            this.SyntaxGenerationContext = syntaxGenerationContext;
            this.LexicalScope = lexicalScope;
            this._proceedExpression = proceedExpression;
            Invariant.Assert( this.DiagnosticSink.DefaultScope != null );
            Invariant.Assert( this.DiagnosticSink.DefaultScope!.Equals( this.MetaApi.Declaration ) );
        }

        public object TemplateInstance { get; }

        public ICompilation Compilation { get; }

        public SyntaxSerializationService SyntaxSerializationService { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public OurSyntaxGenerator SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;

        public StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression )
        {
            if ( returnExpression == null )
            {
                return ReturnStatement();
            }

            var method = this.MetaApi.Method;

            var returnType = method.ReturnType;

            if ( this._templateMethod.MustInterpretAsAsync() )
            {
                // If we are in an awaitable async method, the consider the return type as seen by the method body,
                // not the one as seen from outside.
                var asyncInfo = method.GetAsyncInfoImpl();

                if ( asyncInfo.IsAwaitable )
                {
                    returnType = asyncInfo.ResultType;
                }
            }

            if ( TypeExtensions.Equals( returnType, SpecialType.Void ) )
            {
                return CreateReturnStatementVoid( returnExpression );
            }
            else if ( method.GetIteratorInfoImpl() is { EnumerableKind: EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator } iteratorInfo &&
                      this._templateMethod.MustInterpretAsAsyncIterator() )
            {
                switch ( iteratorInfo.EnumerableKind )
                {
                    case EnumerableKind.IAsyncEnumerable:

                        return this.CreateReturnStatementAsyncEnumerable( returnExpression );

                    case EnumerableKind.IAsyncEnumerator:
                        return this.CreateReturnStatementAsyncEnumerator( returnExpression );

                    default:
                        throw new AssertionFailedException();
                }
            }
            else
            {
                return this.CreateReturnStatementDefault( returnExpression, returnType );
            }
        }

        private StatementSyntax CreateReturnStatementDefault( ExpressionSyntax returnExpression, IType returnType )
        {
            if ( returnExpression.Kind() is SyntaxKind.DefaultLiteralExpression or SyntaxKind.NullLiteralExpression )
            {
                // If we need to return null or default, there is no need to emit a cast.
                return
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                        returnExpression,
                        Token( SyntaxKind.SemicolonToken ) );
            }
            else
            {
                var compilation = returnType.GetCompilationModel().RoslynCompilation;

                if ( RuntimeExpression.TryFindExpressionType( returnExpression, compilation, out var expressionType ) &&
                     compilation.HasImplicitConversion( expressionType, returnType.GetSymbol() ) )
                {
                    // No need to emit a cast.
                    return
                        ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                            returnExpression,
                            Token( SyntaxKind.SemicolonToken ) );
                }
                else
                {
                    return
                        ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                            this.SyntaxGenerator.CastExpression( returnType.GetSymbol(), returnExpression )
                                .WithAdditionalAnnotations( Simplifier.Annotation ),
                            Token( SyntaxKind.SemicolonToken ) );
                }
            }
        }

        private StatementSyntax CreateReturnStatementAsyncEnumerable( ExpressionSyntax returnExpression )
        {
            // We are in an async iterator (or async stream), and we cannot have a return statement.
            // Generate this instead:
            // foreach ( var r in result ) { yield return r; }

            var resultItem = this.LexicalScope.GetUniqueIdentifier( "r" );

            // TODO: Possible optimization
            // We could assume that the result is an AsyncEnumerableList. The class also implements IEnumerable without
            // performance overhead. It is more efficient to do a foreach than an `await foreach`, so we do it.
            // However, the user may have stored the result in a differently-typed variable, so we need to cast.

            var forEach = ForEachStatement(
                    IdentifierName(
                        Identifier(
                            default,
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            default ) ),
                    Identifier( resultItem ),
                    returnExpression,
                    Block(
                        SingletonList<StatementSyntax>(
                            YieldStatement(
                                SyntaxKind.YieldReturnStatement,
                                IdentifierName( resultItem ) ) ) ) )
                .WithAwaitKeyword( Token( SyntaxKind.AwaitKeyword ) )
                .NormalizeWhitespace();

            return Block( forEach, CreateYieldBreakStatement() ).WithFlattenBlockAnnotation();
        }

        private StatementSyntax CreateReturnStatementAsyncEnumerator( ExpressionSyntax returnExpression )
        {
            // We are in an async iterator (or async stream), and we cannot have a return statement.
            // Generate this instead:
            // async using ( var enumerator = METHOD() )
            // {
            //     while ( await enumerator.MoveNextAsync() )
            //     {
            //         yield return enumerator.Current;
            //             
            //         cancellationToken.ThrowIfCancellationRequested();
            //     }
            // }

            // TODO: Possible optimization
            // We could assume that the result is an AsyncEnumerableList. The class also implements IEnumerable without
            // performance overhead. It is more efficient to do a foreach than an `await foreach`, so we do it.
            // However, the user may have stored the result in a differently-typed variable, so we need to cast.

            /*
             * ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("cancellationToken"),
                                            IdentifierName("ThrowIfCancellationRequested"))))
             */

            VariableDeclarationSyntax? local;
            ExpressionSyntax? usingExpression;
            IdentifierNameSyntax? enumeratorIdentifier;
            UsingStatementSyntax usingStatement;

            if ( returnExpression is IdentifierNameSyntax returnIdentifier )
            {
                local = null;
                usingExpression = returnExpression;
                enumeratorIdentifier = returnIdentifier;
            }
            else
            {
                var enumerator = this.LexicalScope.GetUniqueIdentifier( "enumerator" );

                local = VariableDeclaration(
                        IdentifierName(
                            Identifier(
                                default,
                                SyntaxKind.VarKeyword,
                                "var",
                                "var",
                                TriviaList( ElasticSpace ) ) ) )
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator( Identifier( enumerator ) )
                                .WithInitializer( EqualsValueClause( returnExpression ) ) ) );

                usingExpression = null;
                enumeratorIdentifier = IdentifierName( enumerator );
            }

            var whileStatement = WhileStatement(
                AwaitExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            enumeratorIdentifier,
                            IdentifierName( "MoveNextAsync" ) ) ) ),
                Block(
                    YieldStatement(
                        SyntaxKind.YieldReturnStatement,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            enumeratorIdentifier,
                            IdentifierName( "Current" ) ) ) ) );

            usingStatement = UsingStatement(
                Token( SyntaxKind.AwaitKeyword ),
                Token( SyntaxKind.UsingKeyword ),
                Token( SyntaxKind.OpenParenToken ),
                local!,
                usingExpression!,
                Token( SyntaxKind.CloseParenToken ),
                Block( whileStatement ) );

            return Block( usingStatement, CreateYieldBreakStatement() )
                .NormalizeWhitespace()
                .WithFlattenBlockAnnotation();
        }

        private static YieldStatementSyntax CreateYieldBreakStatement()
            => YieldStatement( SyntaxKind.YieldBreakStatement ).WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );

        private static StatementSyntax CreateReturnStatementVoid( ExpressionSyntax? returnExpression )
        {
            // We have a void method, so we cannot a emit a "return X" where X is an expression.
            // However, we have an expression, and it may have a side effect, therefore we cannot just drop X.
            // We try to detect if the can have a side effect. If yes, we add it as an expression statement.

            switch ( returnExpression )
            {
                case InvocationExpressionSyntax invocationExpression:
                    // Do not use discard on invocations, because it may be void.
                    return
                        Block(
                                ExpressionStatement( invocationExpression ),
                                ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

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
                                            TemplateSyntaxFactory.AddSimplifierAnnotations( ParenthesizedExpression( returnExpression ) ) ) ) ),
                                ReturnStatement() )
                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
        }

        public UserDiagnosticSink DiagnosticSink => this.MetaApi.Diagnostics;

        public StatementSyntax CreateReturnStatement( IUserExpression? returnExpression )
        {
            if ( returnExpression == null )
            {
                return ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );
            }
            else if ( TypeExtensions.Equals( returnExpression.Type, SpecialType.Void ) )
            {
                if ( TypeExtensions.Equals( this.MetaApi.Method.ReturnType, SpecialType.Void ) )
                {
                    return
                        Block(
                                ExpressionStatement( returnExpression.ToRunTimeExpression() ),
                                ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                            .AddLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }
                else
                {
                    // TODO: Emit error.
                    throw new AssertionFailedException();
                }
            }
            else
            {
                return this.CreateReturnStatement( returnExpression.ToRunTimeExpression() );
            }
        }

        public IUserExpression? Proceed( string methodName )
        {
            if ( this._proceedExpression == null )
            {
                throw new AssertionFailedException( "No proceed expression was provided." );
            }

            return new ProceedUserExpression( methodName, this );
        }

        private class DisposeCookie : IDisposable
        {
            private readonly Action _action;

            public DisposeCookie( Action action )
            {
                this._action = action;
            }

            public void Dispose() => this._action();
        }
    }
}