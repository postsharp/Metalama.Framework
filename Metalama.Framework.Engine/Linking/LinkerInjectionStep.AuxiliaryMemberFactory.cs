﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using static Metalama.Framework.Engine.Templating.SyntaxFactoryEx;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private sealed class AuxiliaryMemberFactory
    {
        private readonly LinkerInjectionStep _parent;
        private readonly CompilationModel _finalCompilationModel;
        private readonly LexicalScopeFactory _lexicalScopeFactory;
        private readonly AspectReferenceSyntaxProvider _aspectReferenceSyntaxProvider;
        private readonly LinkerInjectionNameProvider _injectionNameProvider;
        private readonly LinkerInjectionHelperProvider _injectionHelperProvider;
        private readonly TransformationCollection _transformationCollection;

        public AuxiliaryMemberFactory(
            LinkerInjectionStep parent,
            CompilationModel finalCompilationModel,
            LexicalScopeFactory lexicalScopeFactory,
            AspectReferenceSyntaxProvider aspectReferenceSyntaxProvider,
            LinkerInjectionNameProvider linkerInjectionNameProvider,
            LinkerInjectionHelperProvider injectionHelperProvider,
            TransformationCollection transformationCollection )
        {
            this._parent = parent;
            this._finalCompilationModel = finalCompilationModel;
            this._lexicalScopeFactory = lexicalScopeFactory;
            this._aspectReferenceSyntaxProvider = aspectReferenceSyntaxProvider;
            this._injectionNameProvider = linkerInjectionNameProvider;
            this._injectionHelperProvider = injectionHelperProvider;
            this._transformationCollection = transformationCollection;
        }

        private CompilationContext CompilationContext => this._parent._compilationContext;

        private SyntaxGenerationOptions SyntaxGenerationOptions => this._parent._syntaxGenerationOptions;

        public ConstructorDeclarationSyntax GetAuxiliarySourceConstructor( IConstructor constructor )
        {
#if ROSLYN_4_8_0_OR_GREATER
            var syntax = (TypeDeclarationSyntax) constructor.GetPrimaryDeclarationSyntax().AssertNotNull();
#else
            var syntax = (RecordDeclarationSyntax) constructor.GetPrimaryDeclarationSyntax().AssertNotNull();
#endif

            var syntaxGenerationContext = this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, syntax );

            var parameters = syntax.ParameterList.AssertNotNull();

            if ( this._transformationCollection.TryGetMemberLevelTransformations( syntax, out var memberTransformations )
                 && memberTransformations.Parameters.Length > 0 )
            {
                parameters =
                    parameters.AddParameters( memberTransformations.Parameters.SelectAsArray( p => p.ToSyntax( syntaxGenerationContext ) ) );
            }

            parameters =
                parameters.AddParameters(
                    Parameter(
                        List<AttributeListSyntax>(),
                        TokenList(),
                        this._injectionHelperProvider.GetSourceType(),
                        Identifier( AspectReferenceSyntaxProvider.LinkerOverrideParamName ),
                        EqualsValueClause(
                            LiteralExpression(
                                SyntaxKind.DefaultLiteralExpression,
                                Token( SyntaxKind.DefaultKeyword ) ) ) ) );

            return ConstructorDeclaration(
                List<AttributeListSyntax>(),
                constructor
                    .GetSyntaxModifierList( ModifierCategories.Unsafe )
                    .Insert( 0, TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) ),
                Identifier( constructor.DeclaringType.Name ),
                parameters,
                ConstructorInitializer(
                    SyntaxKind.ThisConstructorInitializer,
                    ArgumentList(
                        SeparatedList(
                            syntax.ParameterList.Parameters.SelectAsArray(
                                p =>
                                    Argument(
                                        null,
                                        p.Modifiers.FirstOrDefault( m => !m.IsKind( SyntaxKind.ParamsKeyword ) ),
                                        IdentifierName( p.Identifier.ValueText ) ) ) ) ) ),
                Block(),
                null,
                default );
        }

        public MemberDeclarationSyntax GetAuxiliaryContractMember(
            IMember member,
            CompilationModel compilationModel,
            Advice advice,
            string? returnVariableName )
        {
            switch ( member )
            {
                case IMethod method:
                    return this.GetAuxiliaryContractMethod( method, compilationModel, advice.AspectLayerId, returnVariableName );

                case IProperty property:
                    return this.GetAuxiliaryContractProperty( property, advice.AspectLayerId, returnVariableName );

                case IIndexer indexer:
                    return this.GetAuxiliaryContractIndexer( indexer, advice, returnVariableName );

                default:
                    throw new AssertionFailedException( $"Unsupported kind: {member.DeclarationKind}" );
            }
        }

        private MemberDeclarationSyntax GetAuxiliaryContractMethod(
            IMethod method,
            CompilationModel compilationModel,
            AspectLayerId aspectLayerId,
            string? returnVariableName )
        {
            var primaryDeclaration = method.GetPrimaryDeclarationSyntax();

            var syntaxGenerationContext =
                primaryDeclaration != null
                    ? this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, primaryDeclaration )
                    : this.CompilationContext.GetSyntaxGenerationContext(
                        this.SyntaxGenerationOptions,
                        method.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull() );

            var iteratorInfo = method.GetIteratorInfo();
            var asyncInfo = method.GetAsyncInfo();

            // Create proceed expression using common helpers.
            var invocationExpression =
                InvocationExpression(
                    ProceedHelper.CreateMemberAccessExpression( method, aspectLayerId, AspectReferenceTargetKind.Self, syntaxGenerationContext ),
                    ArgumentList(
                        SeparatedList(
                            method.Parameters.SelectAsReadOnlyList(
                                p => Argument( null, InvocationRefKindToken( p.RefKind ), IdentifierName( p.Name ) ) ) ) ) );

            var (useStateMachine, emulatedTemplateKind) = (returnVariableName != null, asyncInfo, iteratorInfo) switch
            {
                (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerable or EnumerableKind.UntypedIEnumerable }) => (
                    false, TemplateKind.IEnumerable),
                (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator }) => (
                    false, TemplateKind.IEnumerator),
                (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerable }) => (false, TemplateKind.IAsyncEnumerable),
                (false, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerator }) => (false, TemplateKind.IAsyncEnumerator),
                (false, { IsAsync: true }, _) when method.ReturnType.Is( SpecialType.Void ) => (true, TemplateKind.Default),
                (false, { IsAsync: true }, _) => (false, TemplateKind.Async),
                (true, _, { IsIteratorMethod: true, EnumerableKind: EnumerableKind.IAsyncEnumerable }) => (true, TemplateKind.IAsyncEnumerable),
                (true, { IsAsync: true }, _) => (true, TemplateKind.Default),
                _ => (false, TemplateKind.Default)
            };

            var proceedExpression = ProceedHelper.CreateProceedExpression( syntaxGenerationContext, invocationExpression, emulatedTemplateKind, method ).Syntax;

            var modifiers = method
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Async | ModifierCategories.Unsafe )
                .Insert( 0, TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

            TypeSyntax? returnType = null;
            var isVoidReturning = method.GetAsyncInfo().ResultType.Is( SpecialType.Void );

            if ( !method.IsAsync )
            {
                if ( useStateMachine )
                {
                    // If the template is async but the overridden declaration is not, we have to add an async modifier.
                    modifiers = modifiers.Add( TokenWithTrailingSpace( SyntaxKind.AsyncKeyword ) );
                }
            }
            else
            {
                if ( !useStateMachine )
                {
                    // If the template is not async but the overridden declaration is, we have to remove the async modifier.
                    modifiers = TokenList( modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) );
                }

                // If the template is async and the target declaration is `async void`, and regardless of the async flag the template, we have to change the type to ValueTask, otherwise
                // it is not awaitable

                if ( method.ReturnType.Equals( SpecialType.Void ) )
                {
                    returnType = syntaxGenerationContext.SyntaxGenerator.Type(
                        compilationModel.CompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ValueTask) ) );
                }
            }

            BlockSyntax body;

            if ( returnVariableName != null && !isVoidReturning )
            {
                if ( emulatedTemplateKind is TemplateKind.IEnumerable or TemplateKind.IAsyncEnumerable )
                {
                    var returnItemName = this._lexicalScopeFactory.GetLexicalScope( method ).GetUniqueIdentifier( "returnItem" );

                    body = Block(
                        CreateLocalVariableDeclaration( returnVariableName ),
                        CreateEnumerableEpilogue( returnItemName, IdentifierName( returnVariableName ) ) );
                }
                else if ( iteratorInfo.EnumerableKind is EnumerableKind.IEnumerator or EnumerableKind.UntypedIEnumerator
                         or EnumerableKind.IAsyncEnumerator )
                {
                    // TODO: #34577 This is wrong, the enumerator needs to be cloned/reset.
                    var bufferedEnumeratorName = this._lexicalScopeFactory.GetLexicalScope( method ).GetUniqueIdentifier( "bufferedEnumerator" );

                    body = Block(
                        CreateLocalVariableDeclaration( bufferedEnumeratorName ),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                VarIdentifier(),
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier( returnVariableName ),
                                        null,
                                        EqualsValueClause( IdentifierName( bufferedEnumeratorName ) ) ) ) ) ),
                        CreateEnumeratorEpilogue( IdentifierName( bufferedEnumeratorName ) ) );
                }
                else
                {
                    body = Block(
                        CreateLocalVariableDeclaration( returnVariableName ),
                        ReturnStatement(
                            TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                            IdentifierName( returnVariableName ),
                            Token( SyntaxKind.SemicolonToken ) ) );
                }

                StatementSyntax CreateLocalVariableDeclaration( string variableName )
                {
                    return LocalDeclarationStatement(
                        VariableDeclaration(
                            VarIdentifier(),
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier( variableName ),
                                    null,
                                    EqualsValueClause( proceedExpression ) ) ) ) );
                }
            }
            else if ( !isVoidReturning )
            {
                body = Block(
                    ReturnStatement(
                        TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                        proceedExpression,
                        Token( SyntaxKind.SemicolonToken ) ) );
            }
            else
            {
                body = Block( ExpressionStatement( proceedExpression ) );
            }

            returnType ??= syntaxGenerationContext.SyntaxGenerator.Type( method.ReturnType.GetSymbol() );

            return MethodDeclaration(
                List<AttributeListSyntax>(),
                modifiers,
                returnType.WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ),
                null,
                Identifier( this._injectionNameProvider.GetOverrideName( method.DeclaringType, aspectLayerId, method ) ),
                syntaxGenerationContext.SyntaxGenerator.TypeParameterList( method, compilationModel ),
                syntaxGenerationContext.SyntaxGenerator.ParameterList( method, compilationModel, true ),
                syntaxGenerationContext.SyntaxGenerator.ConstraintClauses( method ),
                body,
                null );

            StatementSyntax CreateEnumerableEpilogue( string itemName, ExpressionSyntax enumerableExpression )
            {
                return
                    ForEachStatement(
                        List<AttributeListSyntax>(),
                        method.IsAsync
                            ? Token( TriviaList(), SyntaxKind.AwaitKeyword, TriviaList( ElasticSpace ) )
                            : default,
                        Token( SyntaxKind.ForEachKeyword ),
                        Token( SyntaxKind.OpenParenToken ),
                        VarIdentifier(),
                        Identifier( itemName ),
                        Token( TriviaList( ElasticSpace ), SyntaxKind.InKeyword, TriviaList( ElasticSpace ) ),
                        enumerableExpression,
                        Token( SyntaxKind.CloseParenToken ),
                        Block(
                            YieldStatement(
                                SyntaxKind.YieldReturnStatement,
                                Token( TriviaList(), SyntaxKind.YieldKeyword, TriviaList( ElasticSpace ) ),
                                Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                IdentifierName( itemName ),
                                Token( SyntaxKind.SemicolonToken ) ) ) );
            }

            StatementSyntax CreateEnumeratorEpilogue( ExpressionSyntax enumeratorExpression )
            {
                ExpressionSyntax moveNextExpression =
                    method.IsAsync
                        ? AwaitExpression(
                            Token( TriviaList(), SyntaxKind.AwaitKeyword, TriviaList( ElasticSpace ) ),
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    enumeratorExpression,
                                    IdentifierName( "MoveNextAsync" ) ) ) )
                        : InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                enumeratorExpression,
                                IdentifierName( "MoveNext" ) ) );

                return
                    WhileStatement(
                        List<AttributeListSyntax>(),
                        Token( TriviaList(), SyntaxKind.WhileKeyword, TriviaList() ),
                        Token( TriviaList(), SyntaxKind.OpenParenToken, TriviaList() ),
                        moveNextExpression,
                        Token( TriviaList(), SyntaxKind.CloseParenToken, TriviaList() ),
                        Block(
                            YieldStatement(
                                SyntaxKind.YieldReturnStatement,
                                Token( TriviaList(), SyntaxKind.YieldKeyword, TriviaList( ElasticSpace ) ),
                                Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    enumeratorExpression,
                                    IdentifierName( "Current" ) ),
                                Token( SyntaxKind.SemicolonToken ) ) ) );
            }
        }

        private MemberDeclarationSyntax GetAuxiliaryContractProperty(
            IProperty property,
            AspectLayerId aspectLayerId,
            string? returnVariableName )
        {
            var primaryDeclaration = property.GetPrimaryDeclarationSyntax();

            var syntaxGenerationContext =
                primaryDeclaration != null
                    ? this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, primaryDeclaration )
                    : this.CompilationContext.GetSyntaxGenerationContext(
                        this.SyntaxGenerationOptions,
                        property.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull() );

            // TODO: Should return expression body when there is no variable, but expression body inliners do not work yet.
            var getAccessorBody =
                property.GetMethod != null
                    ? returnVariableName != null
                        ? Block(
                            LocalDeclarationStatement(
                                VariableDeclaration(
                                    VarIdentifier(),
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier( returnVariableName ),
                                            null,
                                            EqualsValueClause(
                                                this._aspectReferenceSyntaxProvider.GetPropertyReference(
                                                    aspectLayerId,
                                                    property,
                                                    AspectReferenceTargetKind.PropertyGetAccessor,
                                                    syntaxGenerationContext.SyntaxGenerator ) ) ) ) ) ),
                            ReturnStatement(
                                TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                                IdentifierName( returnVariableName ),
                                Token( SyntaxKind.SemicolonToken ) ) )
                        : Block(
                            ReturnStatement(
                                TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                                TransformationHelper.CreatePropertyProceedGetExpression(
                                    this._aspectReferenceSyntaxProvider,
                                    syntaxGenerationContext,
                                    property,
                                    aspectLayerId ),
                                Token( SyntaxKind.SemicolonToken ) ) )
                    : null;

            var setAccessorDeclarationKind = (property.IsStatic, property.Writeability) switch
            {
                (true, not Writeability.None) => SyntaxKind.SetAccessorDeclaration,
                (false, Writeability.ConstructorOnly) =>
                    syntaxGenerationContext.SupportsInitAccessors ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
                (false, Writeability.InitOnly) => SyntaxKind.InitAccessorDeclaration,
                (false, Writeability.All) => SyntaxKind.SetAccessorDeclaration,
                _ => SyntaxKind.None
            };

            // TODO: Should return expression body, but expression body inliners do not work yet.
            var setAccessorBody =
                property.SetMethod != null
                    ? Block(
                        ExpressionStatement(
                            TransformationHelper.CreatePropertyProceedSetExpression(
                                this._aspectReferenceSyntaxProvider,
                                syntaxGenerationContext,
                                property,
                                aspectLayerId ) ) )
                    : null;

            var modifiers = property
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe )
                .Insert( 0, TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

            return
                PropertyDeclaration(
                    List<AttributeListSyntax>(),
                    modifiers,
                    syntaxGenerationContext.SyntaxGenerator.PropertyType( property )
                        .WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ),
                    null,
                    Identifier( this._injectionNameProvider.GetOverrideName( property.DeclaringType, aspectLayerId, property ) ),
                    AccessorList(
                        List(
                            new[]
                            {
                                getAccessorBody != null
                                    ? AccessorDeclaration(
                                        SyntaxKind.GetAccessorDeclaration,
                                        List<AttributeListSyntax>(),
                                        TokenList(),
                                        Token( SyntaxKind.GetKeyword ),
                                        getAccessorBody,
                                        default,
                                        default )
                                    : null,
                                setAccessorBody != null
                                    ? AccessorDeclaration(
                                        setAccessorDeclarationKind,
                                        List<AttributeListSyntax>(),
                                        TokenList(),
                                        setAccessorDeclarationKind == SyntaxKind.SetAccessorDeclaration
                                            ? Token( SyntaxKind.SetKeyword )
                                            : Token( SyntaxKind.InitKeyword ),
                                        setAccessorBody,
                                        default,
                                        default )
                                    : null
                            }.WhereNotNull() ) ),
                    null,
                    null );
        }

        private MemberDeclarationSyntax GetAuxiliaryContractIndexer(
            IIndexer indexer,
            Advice advice,
            string? returnVariableName )
        {
            var primaryDeclaration = indexer.GetPrimaryDeclarationSyntax();

            var syntaxGenerationContext =
                primaryDeclaration != null
                    ? this.CompilationContext.GetSyntaxGenerationContext( this.SyntaxGenerationOptions, primaryDeclaration )
                    : this.CompilationContext.GetSyntaxGenerationContext(
                        this.SyntaxGenerationOptions,
                        indexer.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull() );

            // TODO: Should return expression body when there is no variable, but expression body inliners do not work yet.
            var getAccessorBody =
                indexer.GetMethod != null
                    ? returnVariableName != null
                        ? Block(
                            LocalDeclarationStatement(
                                VariableDeclaration(
                                    VarIdentifier(),
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier( returnVariableName ),
                                            null,
                                            EqualsValueClause(
                                                this._aspectReferenceSyntaxProvider.GetIndexerReference(
                                                    advice.AspectLayerId,
                                                    indexer,
                                                    AspectReferenceTargetKind.PropertyGetAccessor,
                                                    syntaxGenerationContext.SyntaxGenerator ) ) ) ) ) ),
                            ReturnStatement(
                                TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                                IdentifierName( returnVariableName ),
                                Token( SyntaxKind.SemicolonToken ) ) )
                        : Block(
                            ReturnStatement(
                                TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                                TransformationHelper.CreateIndexerProceedGetExpression(
                                    this._aspectReferenceSyntaxProvider,
                                    syntaxGenerationContext,
                                    indexer,
                                    advice.AspectLayerId ),
                                Token( SyntaxKind.SemicolonToken ) ) )
                    : null;

            var setAccessorDeclarationKind = (indexer.IsStatic, indexer.Writeability) switch
            {
                (true, not Writeability.None) => SyntaxKind.SetAccessorDeclaration,
                (false, Writeability.ConstructorOnly) =>
                    syntaxGenerationContext.SupportsInitAccessors ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
                (false, Writeability.InitOnly) => SyntaxKind.InitAccessorDeclaration,
                (false, Writeability.All) => SyntaxKind.SetAccessorDeclaration,
                _ => SyntaxKind.None
            };

            // TODO: Should return expression body, but expression body inliners do not work yet.
            var setAccessorBody =
                indexer.SetMethod != null
                    ? Block(
                        ExpressionStatement(
                            TransformationHelper.CreateIndexerProceedSetExpression(
                                this._aspectReferenceSyntaxProvider,
                                syntaxGenerationContext,
                                indexer,
                                advice.AspectLayerId ) ) )
                    : null;

            var modifiers = indexer
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe )
                .Insert( 0, TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

            return
                IndexerDeclaration(
                    List<AttributeListSyntax>(),
                    modifiers,
                    syntaxGenerationContext.SyntaxGenerator.IndexerType( indexer )
                        .WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ),
                    null,
                    Token( SyntaxKind.ThisKeyword ),
                    TransformationHelper.GetIndexerOverrideParameterList(
                        this._finalCompilationModel,
                        syntaxGenerationContext,
                        indexer,
                        this._injectionNameProvider.GetAuxiliaryType( advice.Aspect, indexer ) ),
                    AccessorList(
                        List(
                            new[]
                                {
                                    getAccessorBody != null
                                        ? AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration,
                                            List<AttributeListSyntax>(),
                                            TokenList(),
                                            Token( SyntaxKind.GetKeyword ),
                                            getAccessorBody,
                                            default,
                                            default )
                                        : null,
                                    setAccessorBody != null
                                        ? AccessorDeclaration(
                                            setAccessorDeclarationKind,
                                            List<AttributeListSyntax>(),
                                            TokenList(),
                                            setAccessorDeclarationKind == SyntaxKind.SetAccessorDeclaration
                                                ? Token( SyntaxKind.SetKeyword )
                                                : Token( SyntaxKind.InitKeyword ),
                                            setAccessorBody,
                                            default,
                                            default )
                                        : null
                                }.Where( a => a != null )
                                .AssertNoneNull() ) ),
                    null,
                    default );
        }
    }
}