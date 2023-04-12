// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed class LinkerAspectReferenceSyntaxProvider : AspectReferenceSyntaxProvider
    {
        public override ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer )
            => InvocationExpression(
                LinkerInjectionHelperProvider.GetFinalizeMemberExpression()
                    .WithAspectReferenceAnnotation(
                        aspectLayer,
                        AspectReferenceOrder.Previous,
                        flags: AspectReferenceFlags.Inlineable ) );

        public override ExpressionSyntax GetPropertyReference(
            AspectLayerId aspectLayer,
            IProperty overriddenProperty,
            AspectReferenceTargetKind targetKind,
            OurSyntaxGenerator syntaxGenerator )
        {
            switch (targetKind, overriddenProperty)
            {
                case (AspectReferenceTargetKind.PropertySetAccessor, { SetMethod: IPseudoDeclaration }):
                case (AspectReferenceTargetKind.PropertyGetAccessor, { GetMethod: IPseudoDeclaration }):
                    // For pseudo source: __LinkerInjectionHelpers__.__Property(<property_expression>)
                    // It is important to track the <property_expression>.
                    var symbolSourceExpression = CreateMemberAccessExpression( overriddenProperty, syntaxGenerator );

                    return
                        InvocationExpression(
                            LinkerInjectionHelperProvider.GetPropertyMemberExpression()
                                .WithAspectReferenceAnnotation(
                                    aspectLayer,
                                    AspectReferenceOrder.Previous,
                                    targetKind,
                                    flags: AspectReferenceFlags.Inlineable ),
                            ArgumentList( SingletonSeparatedList( Argument( symbolSourceExpression ) ) ) );

                default:
                    // Otherwise: <property_expression>
                    return
                        CreateMemberAccessExpression( overriddenProperty, syntaxGenerator )
                            .WithAspectReferenceAnnotation(
                                aspectLayer,
                                AspectReferenceOrder.Previous,
                                targetKind,
                                AspectReferenceFlags.Inlineable );
            }
        }

        public override ExpressionSyntax GetIndexerReference(
            AspectLayerId aspectLayer,
            IIndexer overriddenIndexer,
            AspectReferenceTargetKind targetKind,
            OurSyntaxGenerator syntaxGenerator )
        {
            return
                ElementAccessExpression(
                        CreateIndexerAccessExpression( overriddenIndexer, syntaxGenerator ),
                        BracketedArgumentList(
                            SeparatedList(
                                overriddenIndexer.Parameters.SelectAsEnumerable(
                                    p =>
                                    {
                                        var refKind = p.RefKind switch
                                        {
                                            RefKind.None => default,
                                            RefKind.In => default,
                                            RefKind.Out => Token( SyntaxKind.OutKeyword ),
                                            RefKind.Ref => Token( SyntaxKind.RefKeyword ),
                                            _ => throw new AssertionFailedException( $"Unexpected RefKind: {p.RefKind}." )
                                        };

                                        return Argument( null, refKind, IdentifierName( p.Name ) );
                                    } ) ) ) )
                    .WithAspectReferenceAnnotation(
                        aspectLayer,
                        AspectReferenceOrder.Previous,
                        targetKind,
                        AspectReferenceFlags.Inlineable );
        }

        public override ExpressionSyntax GetOperatorReference( AspectLayerId aspectLayer, IMethod overriddenOperator, OurSyntaxGenerator syntaxGenerator )
        {
            return
                InvocationExpression(
                    LinkerInjectionHelperProvider.GetOperatorMemberExpression(
                            syntaxGenerator,
                            overriddenOperator.OperatorKind,
                            overriddenOperator.ReturnType,
                            overriddenOperator.Parameters.SelectAsEnumerable( p => p.Type ) )
                        .WithAspectReferenceAnnotation(
                            aspectLayer,
                            AspectReferenceOrder.Previous,
                            flags: AspectReferenceFlags.Inlineable ),
                    syntaxGenerator.ArgumentList( overriddenOperator, p => IdentifierName( p.Name ) ) );
        }

        public override ExpressionSyntax GetEventFieldInitializerExpression( ExpressionSyntax initializerExpression )
        {
            return
                InvocationExpression(
                    LinkerInjectionHelperProvider.GetEventFieldInitializerExpressionMemberExpression(),
                    ArgumentList( SingletonSeparatedList( Argument( null, default, initializerExpression ) ) ) );
        }

        private static ExpressionSyntax CreateIndexerAccessExpression( IIndexer overriddenIndexer, OurSyntaxGenerator syntaxGenerator )
        {
            ExpressionSyntax expression;

            if ( overriddenIndexer.IsExplicitInterfaceImplementation )
            {
                var implementedInterfaceMember = overriddenIndexer.GetExplicitInterfaceImplementation();

                expression =
                    ParenthesizedExpression(
                        SyntaxFactoryEx.SafeCastExpression(
                            syntaxGenerator.Type( implementedInterfaceMember.DeclaringType.GetSymbol() ),
                            ThisExpression() ) );
            }
            else
            {
                expression = ThisExpression();
            }

            return expression;
        }

        private static ExpressionSyntax CreateMemberAccessExpression( IMember overriddenDeclaration, OurSyntaxGenerator syntaxGenerator )
        {
            ExpressionSyntax expression;

            var memberNameString =
                overriddenDeclaration switch
                {
                    { IsExplicitInterfaceImplementation: true } => overriddenDeclaration.Name.Split( '.' ).Last(),
                    _ => overriddenDeclaration.Name
                };

            SimpleNameSyntax memberName;

            if ( overriddenDeclaration is IGeneric { TypeParameters.Count: > 0 } generic )
            {
                memberName = GenericName( memberNameString )
                    .WithTypeArgumentList(
                        TypeArgumentList( SeparatedList( generic.TypeParameters.SelectAsEnumerable( p => (TypeSyntax) IdentifierName( p.Name ) ) ) ) );
            }
            else
            {
                memberName = IdentifierName( memberNameString );
            }

            if ( !overriddenDeclaration.IsStatic )
            {
                if ( overriddenDeclaration.IsExplicitInterfaceImplementation )
                {
                    var implementedInterfaceMember = overriddenDeclaration.GetExplicitInterfaceImplementation();

                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            SyntaxFactoryEx.SafeCastExpression(
                                syntaxGenerator.Type( implementedInterfaceMember.DeclaringType.GetSymbol() ),
                                ThisExpression() ) ),
                        memberName );
                }
                else
                {
                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        memberName );
                }
            }
            else
            {
                expression =
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        syntaxGenerator.Type( overriddenDeclaration.DeclaringType.GetSymbol() ),
                        memberName );
            }

            return expression;
        }
    }
}