using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using FakeItEasy;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.UnitTests.Linker
{
    public class LinkerTestBase : TestBase
    {
        internal static INonObservableTransformation CreateFakeOverride( AspectPartId aspectPart, IMethod targetMethod, MemberDeclarationSyntax overrideSyntax )
        {
            var transformation = (IMemberIntroduction) A.Fake<object>( o => o.Strict().Implements<INonObservableTransformation>().Implements<IMemberIntroduction>().Implements<IOverriddenElement>() );

            A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
            A.CallTo( () => transformation.InsertPositionNode ).Returns( targetMethod.ToSyntaxNode<MemberDeclarationSyntax>() );
            A.CallTo( () => transformation.TargetSyntaxTree ).Returns( targetMethod.ToSyntaxNode<MemberDeclarationSyntax>().SyntaxTree );
            A.CallTo( () => transformation.GetIntroducedMembers() ).Returns(
                new[] { new IntroducedMember( transformation, overrideSyntax, aspectPart, IntroducedMemberSemantic.MethodOverride ) } );
            A.CallTo( () => ((IOverriddenElement) transformation).OverriddenElement ).Returns( targetMethod );

            return (INonObservableTransformation) transformation;
        }

        internal static MemberDeclarationSyntax CreateOverrideSyntax( AspectPartId aspectPart, IMethod targetMethod )
        {
            var originalSyntax = targetMethod.ToSyntaxNode<MethodDeclarationSyntax>();
            var invocation =
                InvocationExpression(
                    !targetMethod.IsStatic
                    ? MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( targetMethod.Name ) )
                    : IdentifierName( targetMethod.Name )
                    )
                .AddLinkerAnnotation( new LinkerAnnotation( aspectPart.AspectType, aspectPart.PartName, LinkerAnnotationOrder.Default ) );

            return MethodDeclaration(
                List<AttributeListSyntax>(),
                originalSyntax.Modifiers,
                originalSyntax.ReturnType,
                originalSyntax.ExplicitInterfaceSpecifier,
                Identifier( $"__{targetMethod.Name}__{aspectPart.AspectType}" ),
                originalSyntax.TypeParameterList,
                originalSyntax.ParameterList,
                originalSyntax.ConstraintClauses,
                Block(
                    targetMethod.GetSymbol().ReturnsVoid
                    ? ExpressionStatement( invocation )
                    : ReturnStatement( invocation )
                    ),
                null
                );
        }

        internal static AspectType CreateFakeAspectType( params string[] aspectParts )
        {
            var aspectCodeType = A.Fake<INamedType>( o => o.Strict() );
            A.CallTo( () => aspectCodeType.FullName ).Returns( "TestAspect" );
            var aspectDriver = A.Fake<IAspectDriver>( o => o.Strict() );

            return new AspectType( aspectCodeType, aspectDriver, new string?[] { null }.Concat( aspectParts ) );
        }
    }
}
