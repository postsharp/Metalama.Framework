// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating;

internal partial class TemplateExpansionContext : UserCodeExecutionContext
{
    private readonly TemplateMember<IMethod>? _template;
    private readonly IUserExpression? _proceedExpression;
    private static readonly AsyncLocal<SyntaxGenerationContext?> _currentSyntaxGenerationContext = new();

    internal static new TemplateExpansionContext Current => CurrentOrNull as TemplateExpansionContext ?? throw new InvalidOperationException();

    /// <summary>
    /// Gets the current <see cref="SyntaxGenerationContext"/>.
    /// </summary>
    internal static SyntaxGenerationContext CurrentSyntaxGenerationContext
        => (CurrentOrNull as TemplateExpansionContext)?.SyntaxGenerationContext
           ?? _currentSyntaxGenerationContext.Value
           ?? throw new InvalidOperationException( "TemplateExpansionContext.CurrentSyntaxGenerationContext has not be set." );

    internal static IDeclaration? CurrentTargetDeclaration => (CurrentOrNull as TemplateExpansionContext)?.TargetDeclaration;

    /// <summary>
    /// Sets the <see cref="CurrentSyntaxGenerationContext"/> but not the <see cref="Current"/> property.
    /// This method is used in tests, when the <see cref="CurrentSyntaxGenerationContext"/> property is needed but not the <see cref="Current"/>
    /// one.
    /// </summary>
    internal static IDisposable WithTestingContext( SyntaxGenerationContext generationContext, ProjectServiceProvider serviceProvider )
    {
        var handle = WithContext( new UserCodeExecutionContext( serviceProvider, NullDiagnosticAdder.Instance, default, Aspects.AspectLayerId.Null ) );
        _currentSyntaxGenerationContext.Value = generationContext;

        return new DisposeCookie(
            () =>
            {
                handle.Dispose();
                _currentSyntaxGenerationContext.Value = null;
            } );
    }

    public TemplateLexicalScope LexicalScope { get; }

    public ITemplateSyntaxFactory SyntaxFactory { get; }

    public TemplateExpansionContext(
        object templateInstance, // This is supposed to be an ITemplateProvider, but we may get different objects in tests.
        MetaApi metaApi,
        TemplateLexicalScope lexicalScope,
        SyntaxSerializationService syntaxSerializationService,
        SyntaxGenerationContext syntaxGenerationContext,
        TemplateMember<IMethod>? template,
        IUserExpression? proceedExpression,
        AspectLayerId aspectLayerId ) : base(
        syntaxGenerationContext.ServiceProvider,
        metaApi.Diagnostics,
        UserCodeMemberInfo.FromSymbol( template?.Declaration.GetSymbol() ),
        aspectLayerId,
        (CompilationModel?) metaApi.Compilation,
        metaApi.Target.Declaration,
        metaApi: metaApi )
    {
        this._template = template;
        this.TemplateInstance = templateInstance;
        this.SyntaxSerializationService = syntaxSerializationService;
        this.SyntaxSerializationContext = new SyntaxSerializationContext( (CompilationModel) metaApi.Compilation, syntaxGenerationContext );
        this.SyntaxGenerationContext = syntaxGenerationContext;
        this.LexicalScope = lexicalScope;
        this._proceedExpression = proceedExpression;
        this.SyntaxFactory = new TemplateSyntaxFactoryImpl( this );
        this.SerializableTypeIdProvider = metaApi.Compilation.GetCompilationModel().CompilationServices.SerializableTypeIdProvider;
    }

    public object TemplateInstance { get; }

    public SyntaxSerializationService SyntaxSerializationService { get; }

    public SyntaxSerializationContext SyntaxSerializationContext { get; }

    public SyntaxGenerationContext SyntaxGenerationContext { get; }

    public SerializableTypeIdProvider SerializableTypeIdProvider { get; }

    public OurSyntaxGenerator SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;

    public new MetaApi MetaApi => base.MetaApi!;

    public StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression, bool awaitResult )
    {
        if ( returnExpression == null )
        {
            Invariant.Assert( !awaitResult );

            return ReturnStatement();
        }

        if ( this.MetaApi.Declaration is IField field )
        {
            // This is field initializer template expansion.
            Invariant.Assert( !awaitResult );

            return this.CreateReturnStatementDefault( returnExpression, field.Type, false );
        }
        else if ( this.MetaApi.Declaration is IProperty property )
        {
            // This is property initializer template expansion.
            Invariant.Assert( !awaitResult );

            return this.CreateReturnStatementDefault( returnExpression, property.Type, false );
        }
        else if ( this.MetaApi.Declaration is IEvent @event )
        {
            // This is event initializer template expansion.
            Invariant.Assert( !awaitResult );

            return this.CreateReturnStatementDefault( returnExpression, @event.Type, false );
        }
        else
        {
            var method = this.MetaApi.Method;
            var returnType = method.ReturnType;

            if ( this._template != null && this._template.MustInterpretAsAsyncTemplate() )
            {
                // If we are in an awaitable async method, the consider the return type as seen by the method body,
                // not the one as seen from outside.
                var asyncInfo = method.GetAsyncInfoImpl();

                if ( asyncInfo.IsAwaitableOrVoid )
                {
                    returnType = asyncInfo.ResultType;
                }
            }

            if ( returnType.Equals( SpecialType.Void ) )
            {
                return CreateReturnStatementVoid( returnExpression );
            }
            else if ( method.GetIteratorInfoImpl() is { EnumerableKind: EnumerableKind.IAsyncEnumerable or EnumerableKind.IAsyncEnumerator } iteratorInfo &&
                      this._template != null && this._template.MustInterpretAsAsyncIteratorTemplate() )
            {
                switch ( iteratorInfo.EnumerableKind )
                {
                    case EnumerableKind.IAsyncEnumerable:

                        return this.CreateReturnStatementAsyncEnumerable( returnExpression );

                    case EnumerableKind.IAsyncEnumerator:
                        return this.CreateReturnStatementAsyncEnumerator( returnExpression );

                    default:
                        throw new AssertionFailedException( $"Unexpected EnumerableKind: {iteratorInfo.EnumerableKind}." );
                }
            }
            else
            {
                return this.CreateReturnStatementDefault( returnExpression, returnType, awaitResult );
            }
        }
    }

    private StatementSyntax CreateReturnStatementDefault( ExpressionSyntax returnExpression, IType returnType, bool awaitResult )
    {
        if ( returnExpression.Kind() is SyntaxKind.DefaultLiteralExpression or SyntaxKind.NullLiteralExpression )
        {
            // If we need to return null or default, there is no need to emit a cast.
            Invariant.Assert( !awaitResult );

            return
                ReturnStatement(
                    Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                    returnExpression,
                    Token( SyntaxKind.SemicolonToken ) );
        }
        else
        {
            var compilation = returnType.GetCompilationModel().RoslynCompilation;

            if ( ExpressionTypeAnnotationHelper.TryFindTypeFromAnnotation( returnExpression, compilation, out var expressionType ) &&
                 compilation.HasImplicitConversion( expressionType, returnType.GetSymbol() ) )
            {
                // No need to emit a cast.
                return
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        awaitResult
                            ? AwaitExpression( Token( SyntaxKind.AwaitKeyword ).WithTrailingTrivia( ElasticSpace ), returnExpression )
                            : returnExpression,
                        Token( SyntaxKind.SemicolonToken ) );
            }
            else
            {
                return
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        this.SyntaxGenerator.CastExpression(
                                returnType.GetSymbol(),
                                awaitResult
                                    ? AwaitExpression( Token( SyntaxKind.AwaitKeyword ).WithTrailingTrivia( ElasticSpace ), returnExpression )
                                    : returnExpression )
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

        var usingStatement = UsingStatement(
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
            case ConditionalAccessExpressionSyntax:
            case InvocationExpressionSyntax:
                // Do not use discard on invocations, because it may be void.
                return
                    Block(
                            ExpressionStatement( returnExpression ),
                            ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            case null:
            case LiteralExpressionSyntax:
            case IdentifierNameSyntax:
                // No need to call the expression  because we are guaranteed to have no side effect and we don't 
                // care about the value.
                return ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );

            case AwaitExpressionSyntax awaitExpression:
                // We have to await in a statement, then return in another statement.
                return Block(
                        ExpressionStatement( awaitExpression ),
                        ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                    .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

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
                                    SyntaxFactoryEx.SafeCastExpression(
                                        PredefinedType( Token( SyntaxKind.ObjectKeyword ) ),
                                        ParenthesizedExpression( returnExpression ).WithSimplifierAnnotation() ) ) ),
                            ReturnStatement() )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
        }
    }

    public UserDiagnosticSink DiagnosticSink => this.MetaApi.Diagnostics;

    public StatementSyntax CreateReturnStatement( IUserExpression? returnUserExpression, bool awaitResult )
    {
        if ( returnUserExpression == null )
        {
            return ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation );
        }
        else if ( returnUserExpression.Type.Equals( SpecialType.Void ) )
        {
            if ( this.MetaApi.Method.ReturnType.Equals( SpecialType.Void )
                 || this.MetaApi.Method.ReturnType.GetAsyncInfo().ResultType.Equals( SpecialType.Void ) )
            {
                var returnExpression = returnUserExpression
                    .ToExpressionSyntax( this.SyntaxGenerationContext )
                    .RemoveParenthesis();

                return
                    Block(
                            ExpressionStatement( returnExpression ),
                            ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
            else
            {
                // TODO: Emit error.
                throw new AssertionFailedException( $"The return expression `{returnUserExpression}` is not void." );
            }
        }
        else if ( awaitResult && returnUserExpression.Type.GetAsyncInfo().ResultType.Equals( SpecialType.Void ) )
        {
            Invariant.Assert( this._template != null && this._template.MustInterpretAsAsyncTemplate() );

            if ( this.MetaApi.Method.ReturnType.Equals( SpecialType.Void )
                 || this.MetaApi.Method.ReturnType.GetAsyncInfo().ResultType.Equals( SpecialType.Void ) )
            {
                return
                    Block(
                            ExpressionStatement( AwaitExpression( returnUserExpression.ToExpressionSyntax( this.SyntaxGenerationContext ) ) ),
                            ReturnStatement().WithAdditionalAnnotations( OutputCodeFormatter.PossibleRedundantAnnotation ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
            }
            else
            {
                // TODO: Emit error.
                throw new AssertionFailedException( $"The Task value of the return expression `{returnUserExpression}` is not void." );
            }
        }
        else
        {
            return this.CreateReturnStatement( returnUserExpression.ToExpressionSyntax( this.SyntaxGenerationContext ), awaitResult );
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