﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

namespace Metalama.Framework.Engine.Linking;

internal sealed class LinkerAspectReferenceSyntaxProvider : AspectReferenceSyntaxProvider
{
    public override ExpressionSyntax GetFinalizerReference( AspectLayerId aspectLayer )
        => InvocationExpression(
            LinkerInjectionHelperProvider.GetFinalizeMemberExpression()
                .WithAspectReferenceAnnotation(
                    null,
                    aspectLayer,
                    AspectReferenceOrder.Previous,
                    flags: AspectReferenceFlags.Inlineable ) );

    public override ExpressionSyntax GetStaticConstructorReference( AspectLayerId aspectLayer )
        => InvocationExpression(
            LinkerInjectionHelperProvider.GetStaticConstructorMemberExpression()
                .WithAspectReferenceAnnotation(
                    null,
                    aspectLayer,
                    AspectReferenceOrder.Previous,
                    flags: AspectReferenceFlags.Inlineable ),
            ArgumentList() );

    public override ExpressionSyntax GetConstructorReference(
        AspectLayerId aspectLayer,
        IConstructor overriddenConstructor,
        OurSyntaxGenerator syntaxGenerator )
        => InvocationExpression(
            LinkerInjectionHelperProvider.GetConstructorMemberExpression()
                .WithAspectReferenceAnnotation(
                    overriddenConstructor.ToSerializableId(),
                    aspectLayer,
                    AspectReferenceOrder.Previous,
                    flags: AspectReferenceFlags.Inlineable ),
            ArgumentList(
                SingletonSeparatedList(
                    Argument(
                        ObjectCreationExpression(
                            syntaxGenerator.Type( overriddenConstructor.DeclaringType.GetSymbol() ),
                            ArgumentList( SeparatedList( overriddenConstructor.Parameters.SelectAsArray( p => Argument( IdentifierName( p.Name ) ) ) ) ),
                            null ) ) ) ) );

    public override ExpressionSyntax GetPropertyReference(
        AspectLayerId aspectLayer,
        IProperty targetProperty,
        AspectReferenceTargetKind targetKind,
        OurSyntaxGenerator syntaxGenerator )
    {
        switch (targetKind, targetProperty)
        {
            case (AspectReferenceTargetKind.PropertySetAccessor, { SetMethod: IPseudoDeclaration }):
            case (AspectReferenceTargetKind.PropertyGetAccessor, { GetMethod: IPseudoDeclaration }):
                // For pseudo source: __LinkerInjectionHelpers__.__Property(<property_expression>)
                // It is important to track the <property_expression>.
                var symbolSourceExpression = CreateMemberAccessExpression( targetProperty, syntaxGenerator );

                return
                    InvocationExpression(
                        LinkerInjectionHelperProvider.GetPropertyMemberExpression()
                            .WithAspectReferenceAnnotation(
                                targetProperty.ToSerializableId(),
                                aspectLayer,
                                AspectReferenceOrder.Previous,
                                targetKind,
                                AspectReferenceFlags.Inlineable ),
                        ArgumentList( SingletonSeparatedList( Argument( symbolSourceExpression ) ) ) );

            default:
                // Otherwise: <property_expression>
                return
                    CreateMemberAccessExpression( targetProperty, syntaxGenerator )
                        .WithAspectReferenceAnnotation(
                            targetProperty.ToSerializableId(),
                            aspectLayer,
                            AspectReferenceOrder.Previous,
                            targetKind,
                            AspectReferenceFlags.Inlineable );
        }
    }

    public override ExpressionSyntax GetIndexerReference(
        AspectLayerId aspectLayer,
        IIndexer targetIndexer,
        AspectReferenceTargetKind targetKind,
        OurSyntaxGenerator syntaxGenerator )
        => ElementAccessExpression(
                CreateIndexerAccessExpression( targetIndexer, syntaxGenerator ),
                BracketedArgumentList(
                    SeparatedList(
                        targetIndexer.Parameters.SelectAsReadOnlyList(
                            p => Argument( null, SyntaxFactoryEx.InvocationRefKindToken( p.RefKind ), IdentifierName( p.Name ) ) ) ) ) )
            .WithAspectReferenceAnnotation(
                targetIndexer.ToSerializableId(),
                aspectLayer,
                AspectReferenceOrder.Previous,
                targetKind,
                AspectReferenceFlags.Inlineable );

    public override ExpressionSyntax GetOperatorReference( AspectLayerId aspectLayer, IMethod targetOperator, OurSyntaxGenerator syntaxGenerator )
        => InvocationExpression(
            LinkerInjectionHelperProvider.GetOperatorMemberExpression(
                    syntaxGenerator,
                    targetOperator.OperatorKind,
                    targetOperator.ReturnType,
                    targetOperator.Parameters.SelectAsReadOnlyList( p => p.Type ) )
                .WithAspectReferenceAnnotation(
                    targetOperator.ToSerializableId(),
                    aspectLayer,
                    AspectReferenceOrder.Previous,
                    flags: AspectReferenceFlags.Inlineable ),
            syntaxGenerator.ArgumentList( targetOperator, p => IdentifierName( p.Name ) ) );

    public override ExpressionSyntax GetEventFieldInitializerExpression( TypeSyntax eventFieldType, ExpressionSyntax initializerExpression )
        => InvocationExpression(
            LinkerInjectionHelperProvider.GetEventFieldInitializerExpressionMemberExpression( eventFieldType ),
            ArgumentList( SingletonSeparatedList( Argument( null, default, initializerExpression ) ) ) );

    private static ExpressionSyntax CreateIndexerAccessExpression( IIndexer targetIndexer, OurSyntaxGenerator syntaxGenerator )
    {
        ExpressionSyntax expression;

        if ( targetIndexer.IsExplicitInterfaceImplementation )
        {
            var implementedInterfaceMember = targetIndexer.GetExplicitInterfaceImplementation();

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

    private static ExpressionSyntax CreateMemberAccessExpression( IMember targetDeclaration, OurSyntaxGenerator syntaxGenerator )
    {
        ExpressionSyntax expression;

        var memberNameString =
            targetDeclaration switch
            {
                { IsExplicitInterfaceImplementation: true } => targetDeclaration.Name.Split( '.' ).Last(),
                _ => targetDeclaration.Name
            };

        SimpleNameSyntax memberName;

        if ( targetDeclaration is IGeneric { TypeParameters.Count: > 0 } generic )
        {
            memberName = GenericName( memberNameString )
                .WithTypeArgumentList(
                    TypeArgumentList( SeparatedList( generic.TypeParameters.SelectAsReadOnlyList( p => (TypeSyntax) IdentifierName( p.Name ) ) ) ) );
        }
        else
        {
            memberName = IdentifierName( memberNameString );
        }

        if ( !targetDeclaration.IsStatic )
        {
            if ( targetDeclaration.IsExplicitInterfaceImplementation )
            {
                var implementedInterfaceMember = targetDeclaration.GetExplicitInterfaceImplementation();

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
                    syntaxGenerator.Type( targetDeclaration.DeclaringType.GetSymbol() ),
                    memberName );
        }

        return expression;
    }
}