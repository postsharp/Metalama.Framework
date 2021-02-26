// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.UnitTests.Linker
{
    public class LinkerTestBase : TestBase
    {
        internal static INonObservableTransformation CreateFakeMethodOverride( AspectLayerId aspectLayer, IMethod targetMethod, MemberDeclarationSyntax overrideSyntax )
        {
            var transformation = (IMemberIntroduction) A.Fake<object>( o => o.Strict().Implements<INonObservableTransformation>().Implements<IMemberIntroduction>().Implements<IOverriddenElement>().Implements<ICodeElement>() );

            A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
            A.CallTo( () => transformation.ToString() ).Returns( "Override" );
            A.CallTo( () => transformation.InsertPositionNode ).Returns( targetMethod.ToSyntaxNode<MemberDeclarationSyntax>() );
            A.CallTo( () => transformation.TargetSyntaxTree ).Returns( targetMethod.ToSyntaxNode<MemberDeclarationSyntax>().SyntaxTree );
            A.CallTo( () => transformation.GetIntroducedMembers( A<MemberIntroductionContext>.Ignored ) ).Returns(
                new[] { new IntroducedMember( transformation, overrideSyntax, aspectLayer, IntroducedMemberSemantic.MethodOverride ) } );
            A.CallTo( () => ((IOverriddenElement) transformation).OverriddenElement ).Returns( targetMethod );

            return (INonObservableTransformation) transformation;
        }

        internal static IObservableTransformation CreateFakeMethodIntroduction( AspectLayerId aspectLayer, INamedType targetType, MemberDeclarationSyntax overrideSyntax )
        {
            var transformation = (IMemberIntroduction) A.Fake<object>( o => o.Strict().Implements<IObservableTransformation>().Implements<IMemberIntroduction>().Implements<ICodeElement>() );

            A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
            A.CallTo( () => transformation.ToString() ).Returns( "Introduced" );
            A.CallTo( () => transformation.InsertPositionNode ).Returns( targetType.ToSyntaxNode<MemberDeclarationSyntax>() );
            A.CallTo( () => transformation.TargetSyntaxTree ).Returns( targetType.ToSyntaxNode<MemberDeclarationSyntax>().SyntaxTree );
            A.CallTo( () => transformation.GetIntroducedMembers( A<MemberIntroductionContext>.Ignored ) ).Returns(
                new[] { new IntroducedMember( transformation, overrideSyntax, aspectLayer, IntroducedMemberSemantic.MethodOverride ) } );
            A.CallTo( () => ((IObservableTransformation) transformation).ContainingElement ).Returns( targetType );

            return (IObservableTransformation) transformation;
        }

        internal static MemberDeclarationSyntax CreateIntroducedMethodSyntax( bool isStatic, Accessibility accesibility, string returnType, string name, params (string Name, string Type)[] parameters )
        {
            return MethodDeclaration(
                List<AttributeListSyntax>(),
                TokenList( GetModifiers() ),
                ParseTypeName( returnType ),
                null,
                Identifier( name ),
                null,
                ParameterList( SeparatedList( parameters.Select( p => Parameter( List<AttributeListSyntax>(), TokenList(), ParseTypeName( p.Type ), Identifier( p.Name ), null ) ) ) ),
                List<TypeParameterConstraintClauseSyntax>(),
                returnType == "void"
                ? Block()
                : Block(
                    ReturnStatement(
                        LiteralExpression(
                            SyntaxKind.DefaultLiteralExpression,
                            Token( SyntaxKind.DefaultKeyword ) ) ) ),
                null );

            IEnumerable<SyntaxToken> GetModifiers()
            {
                var tokens = new List<SyntaxToken?>();
                tokens.Add( isStatic ? Token( SyntaxKind.StaticKeyword ) : null );
                tokens.Add( accesibility == Accessibility.Public ? Token( SyntaxKind.PublicKeyword ) : null );
                tokens.Add( accesibility == Accessibility.Private ? Token( SyntaxKind.PrivateKeyword ) : null );
                tokens.Add( accesibility == Accessibility.Internal ? Token( SyntaxKind.InternalKeyword ) : null );
                tokens.Add( accesibility == Accessibility.Protected ? Token( SyntaxKind.ProtectedKeyword ) : null );
                tokens.AddRange( accesibility == Accessibility.ProtectedOrInternal ? new SyntaxToken?[] { Token( SyntaxKind.ProtectedKeyword ), Token( SyntaxKind.InternalKeyword ) } : Enumerable.Empty<SyntaxToken?>() );
                tokens.AddRange( accesibility == Accessibility.ProtectedAndInternal ? new SyntaxToken?[] { Token( SyntaxKind.PrivateKeyword ), Token( SyntaxKind.ProtectedKeyword ) } : Enumerable.Empty<SyntaxToken?>() );
                return tokens.Where( token => token != null ).Cast<SyntaxToken>();
            }
        }

        internal static MemberDeclarationSyntax CreateOverrideSyntax( AspectLayerId aspectLayer, IMethod targetMethod )
        {
            var originalSyntax = targetMethod.ToSyntaxNode<MethodDeclarationSyntax>();
            var invocation =
                InvocationExpression(
                    !targetMethod.IsStatic
                    ? MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( targetMethod.Name ) )
                    : IdentifierName( targetMethod.Name ) )
                .AddLinkerAnnotation( new LinkerAnnotation( aspectLayer, LinkerAnnotationOrder.Default ) );

            return MethodDeclaration(
                List<AttributeListSyntax>(),
                originalSyntax.Modifiers,
                originalSyntax.ReturnType,
                originalSyntax.ExplicitInterfaceSpecifier,
                Identifier( $"__{targetMethod.Name}__{aspectLayer.AspectName}" ),
                originalSyntax.TypeParameterList,
                originalSyntax.ParameterList,
                originalSyntax.ConstraintClauses,
                Block(
                    targetMethod.GetSymbol().ReturnsVoid
                    ? ExpressionStatement( invocation )
                    : ReturnStatement( invocation ) ),
                null );
        }

        internal static AspectType CreateFakeAspectType( CompilationModel compilation, params string[] aspectLayers )
        {
            var aspectCodeType = A.Fake<INamedType>( o => o.Implements<ICodeElement>().Strict() );
            A.CallTo( () => aspectCodeType.FullName ).Returns( "TestAspect" );
            A.CallTo( () => ((ICodeElement) aspectCodeType).Compilation ).Returns( compilation );
            var attributeList = A.Fake<IAttributeList>( o => o.Strict() );
            A.CallTo( () => attributeList.GetEnumerator() ).ReturnsLazily( _ => ((IEnumerable<IAttribute>)Array.Empty<IAttribute>()).GetEnumerator() );
            A.CallTo( () => aspectCodeType.Attributes ).Returns( attributeList );

            var aspectDriver = A.Fake<IAspectDriver>( o => o.Strict() );

            return new AspectType( aspectCodeType, null, aspectDriver );
        }
    }
}
