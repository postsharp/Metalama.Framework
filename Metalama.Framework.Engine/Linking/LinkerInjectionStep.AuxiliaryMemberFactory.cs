// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


#if DEBUG
#endif

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerInjectionStep
{
    private class AuxiliaryMemberFactory
    {
        private readonly CompilationContext _compilationContext;
        private readonly LinkerInjectionNameProvider _injectionNameProvider;
        private readonly LinkerInjectionHelperProvider _injectionHelperProvider;
        private readonly TransformationCollection _transformationCollection;

        public AuxiliaryMemberFactory(
            CompilationContext compilationContext, 
            LinkerInjectionNameProvider linkerInjectionNameProvider,
            LinkerInjectionHelperProvider injectionHelperProvider, 
            TransformationCollection transformationCollection )
        {
            this._compilationContext = compilationContext;
            this._injectionNameProvider = linkerInjectionNameProvider;
            this._injectionHelperProvider = injectionHelperProvider;
            this._transformationCollection = transformationCollection;
        }

        public ConstructorDeclarationSyntax GetAuxiliarySourceConstructor( IConstructor constructor )
        {                
#if ROSLYN_4_8_0_OR_GREATER
            var syntax = (TypeDeclarationSyntax) constructor.GetPrimaryDeclarationSyntax().AssertNotNull();
#else
            var syntax = (RecordDeclarationSyntax) constructor.GetPrimaryDeclarationSyntax().AssertNotNull();
#endif

            var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext( syntax );

            var parameters = syntax.ParameterList.AssertNotNull();

            if ( this._transformationCollection.TryGetMemberLevelTransformations( syntax, out var memberTransformations )
                 && memberTransformations.Parameters.Length > 0 )
            {
                parameters =
                    parameters.AddParameters(
                            memberTransformations.Parameters.SelectAsArray( p => p.ToSyntax( syntaxGenerationContext ) ) );
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
                constructor.GetSyntaxModifierList( ModifierCategories.Unsafe )
                .Insert( 0, SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) ),
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

        public MemberDeclarationSyntax GetAuxiliaryOutputContractMember(IMember member, CompilationModel compilationModel, AspectLayerId aspectLayerId, string returnVariableName )
        {
            switch ( member )
            {
                case IMethod method:
                    return this.GetAuxiliaryOutputContractMethod(method, compilationModel, aspectLayerId, returnVariableName );
                case IProperty property:
                    return this.GetAuxiliaryOutputContractProperty( property, compilationModel, aspectLayerId, returnVariableName );
                case IIndexer indexer:
                    return this.GetAuxiliaryOutputContractIndexer( indexer, compilationModel, aspectLayerId, returnVariableName );
                default:
                    throw new AssertionFailedException($"Unsupported kind: {member.DeclarationKind}");
            }
        }

        private MemberDeclarationSyntax GetAuxiliaryOutputContractMethod( IMethod method, CompilationModel compilationModel, AspectLayerId aspectLayerId, string returnVariableName )
        {
            var syntaxGenerationContext = this._compilationContext.GetSyntaxGenerationContext(method.GetPrimaryDeclarationSyntax().AssertNotNull() );

            var invocationExpression =
                InvocationExpression(
                   ProceedHelper.CreateMemberAccessExpression( method, aspectLayerId, AspectReferenceTargetKind.Self, syntaxGenerationContext ),
                   ArgumentList(
                       SeparatedList(
                           method.Parameters.SelectAsReadOnlyList(
                               p => Argument( null, SyntaxFactoryEx.InvocationRefKindToken( p.RefKind ), IdentifierName( p.Name ) ) ) ) ) );

            // TODO: TemplateKind determination.

            var proceedExpression = ProceedHelper.CreateProceedExpression( syntaxGenerationContext, invocationExpression, TemplateKind.Default, method ).Syntax;

            var modifiers = method
                .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Async | ModifierCategories.Unsafe )
                .Insert( 0, SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

            var isAsyncTemplate = false;
            TypeSyntax? returnType = null;
            var isVoidReturning = method.GetAsyncInfo().ResultType.Is( Code.SpecialType.Void );

            if ( !method.IsAsync )
            {
                if ( isAsyncTemplate )
                {
                    // If the template is async but the overridden declaration is not, we have to add an async modifier.
                    modifiers = modifiers.Add( SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.AsyncKeyword ) );
                }
            }
            else
            {
                if ( !isAsyncTemplate )
                {
                    // If the template is not async but the overridden declaration is, we have to remove the async modifier.
                    modifiers = TokenList( modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) );
                }

                // If the template is async and the target declaration is `async void`, and regardless of the async flag the template, we have to change the type to ValueTask, otherwise
                // it is not awaitable

                if ( method.ReturnType.Equals( Code.SpecialType.Void ) )
                {
                    returnType = syntaxGenerationContext.SyntaxGenerator.Type( compilationModel.CompilationContext.ReflectionMapper.GetTypeSymbol( typeof( ValueTask ) ) );
                }
            }

            returnType ??= syntaxGenerationContext.SyntaxGenerator.Type( method.ReturnType.GetSymbol() );

            var body =
                isVoidReturning
                ? Block( ExpressionStatement( proceedExpression ) )
                : Block(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    TriviaList( ElasticSpace ) ) ),
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier( returnVariableName ),
                                    null,
                                    EqualsValueClause( proceedExpression ) ) ) ) ),
                    ReturnStatement(
                        Token( default, SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                        IdentifierName( returnVariableName ),
                        Token( SyntaxKind.SemicolonToken ) ) );

            // TODO: Output contract marker (linker helper).
            // TODO: Epilogues.
            return MethodDeclaration(
                List<AttributeListSyntax>(),
                modifiers,
                returnType.WithTrailingTriviaIfNecessary( ElasticSpace, syntaxGenerationContext.NormalizeWhitespace ),
                null,
                Identifier( this._injectionNameProvider.GetOverrideName( method .DeclaringType, aspectLayerId, method) ),
                syntaxGenerationContext.SyntaxGenerator.TypeParameterList( method, compilationModel ),
                syntaxGenerationContext.SyntaxGenerator.ParameterList( method, compilationModel, removeDefaultValues: true ),
                syntaxGenerationContext.SyntaxGenerator.ConstraintClauses( method ),
                body,
                null );
        }

        private MemberDeclarationSyntax GetAuxiliaryOutputContractProperty( IProperty property, CompilationModel compilationModel, AspectLayerId aspectLayerId, string returnVariableName )
        {
            throw new NotImplementedException();
        }

        private MemberDeclarationSyntax GetAuxiliaryOutputContractIndexer( IIndexer indexer, CompilationModel compilationModel, AspectLayerId aspectLayerId, string returnVariableName )
        {
            throw new NotImplementedException();
        }
    }
}