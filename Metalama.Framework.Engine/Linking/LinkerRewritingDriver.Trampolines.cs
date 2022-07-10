// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// TODO: A lot methods here are called multiple times. Optimize.

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerRewritingDriver
    {
        private static MethodDeclarationSyntax GetTrampolineMethod( MethodDeclarationSyntax method, IMethodSymbol targetSymbol )
        {
            // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.

            return method
                .WithBody( GetBody() )
                .WithModifiers( TokenList( method.Modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) ) )
                .NormalizeWhitespace()
                .WithLeadingTrivia( method.GetLeadingTrivia() )
                .WithTrailingTrivia( method.GetTrailingTrivia() );

            BlockSyntax GetBody()
            {
                var invocation =
                    InvocationExpression(
                        GetInvocationTarget(),
                        ArgumentList( SeparatedList( method.ParameterList.Parameters.Select( x => Argument( IdentifierName( x.Identifier ) ) ) ) ) );

                if ( !targetSymbol.ReturnsVoid )
                {
                    return Block(
                        ReturnStatement(
                            Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                            invocation,
                            Token( SyntaxKind.SemicolonToken ) ) );
                }
                else
                {
                    return Block( ExpressionStatement( invocation ) );
                }

                ExpressionSyntax GetInvocationTarget()
                {
                    if ( targetSymbol.IsStatic )
                    {
                        return IdentifierName( targetSymbol.Name );
                    }
                    else
                    {
                        return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) );
                    }
                }
            }
        }

        private static DestructorDeclarationSyntax GetTrampolineDestructor( DestructorDeclarationSyntax dtor, IMethodSymbol targetSymbol )
        {
            // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.

            return
                dtor
                    .WithBody( GetBody() )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( dtor.GetLeadingTrivia() )
                    .WithTrailingTrivia( dtor.GetTrailingTrivia() );

            BlockSyntax GetBody()
            {
                var invocation =
                    InvocationExpression(
                        MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) ),
                        ArgumentList() );

                return Block(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        invocation,
                        Token( SyntaxKind.SemicolonToken ) ) );
            }
        }

        private static ConversionOperatorDeclarationSyntax GetTrampolineConversionOperator( ConversionOperatorDeclarationSyntax @operator, IMethodSymbol targetSymbol )
        {
            // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.

            return
                @operator
                    .WithBody( GetBody() )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( @operator.GetLeadingTrivia() )
                    .WithTrailingTrivia( @operator.GetTrailingTrivia() );

            BlockSyntax GetBody()
            {
                var invocation =
                    InvocationExpression(
                        IdentifierName( targetSymbol.Name ),
                        ArgumentList() );

                return Block(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        invocation,
                        Token( SyntaxKind.SemicolonToken ) ) );
            }
        }

        private static OperatorDeclarationSyntax GetTrampolineOperator( OperatorDeclarationSyntax @operator, IMethodSymbol targetSymbol )
        {
            // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.

            return
                @operator
                    .WithBody( GetBody() )
                    .NormalizeWhitespace()
                    .WithLeadingTrivia( @operator.GetLeadingTrivia() )
                    .WithTrailingTrivia( @operator.GetTrailingTrivia() );

            BlockSyntax GetBody()
            {
                var invocation =
                    InvocationExpression(
                        IdentifierName( targetSymbol.Name ),
                        ArgumentList() );

                return Block(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                        invocation,
                        Token( SyntaxKind.SemicolonToken ) ) );
            }
        }

        private static PropertyDeclarationSyntax GetTrampolineProperty( PropertyDeclarationSyntax property, IPropertySymbol targetSymbol )
        {
            var getAccessor = property.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.GetAccessorDeclaration );
            var setAccessor = property.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.SetAccessorDeclaration );

            return property
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                                {
                                    getAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.GetAccessorDeclaration,
                                                Block(
                                                    ReturnStatement(
                                                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( ElasticSpace ),
                                                        GetInvocationTarget(),
                                                        Token( SyntaxKind.SemicolonToken ) ) ) )
                                            .NormalizeWhitespace()
                                        : null,
                                    setAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.SetAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                        : null
                                }.Where( a => a != null )
                                .AssertNoneNull() ) ) )
                .WithLeadingTrivia( property.GetLeadingTrivia() )
                .WithTrailingTrivia( property.GetTrailingTrivia() );

            ExpressionSyntax GetInvocationTarget()
            {
                if ( targetSymbol.IsStatic )
                {
                    return IdentifierName( targetSymbol.Name );
                }
                else
                {
                    return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) );
                }
            }
        }

        private static EventDeclarationSyntax GetTrampolineEvent( EventDeclarationSyntax @event, IEventSymbol targetSymbol )
        {
            var addAccessor = @event.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.AddAccessorDeclaration );
            var removeAccessor = @event.AccessorList?.Accessors.SingleOrDefault( x => x.Kind() == SyntaxKind.RemoveAccessorDeclaration );

            return @event
                .WithAccessorList(
                    AccessorList(
                        List(
                            new[]
                                {
                                    addAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.AddAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.AddAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                        : null,
                                    removeAccessor != null
                                        ? AccessorDeclaration(
                                                SyntaxKind.RemoveAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SubtractAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                        : null
                                }.Where( a => a != null )
                                .AssertNoneNull() ) ) )
                .WithLeadingTrivia( @event.GetLeadingTrivia() )
                .WithTrailingTrivia( @event.GetTrailingTrivia() );

            ExpressionSyntax GetInvocationTarget()
            {
                if ( targetSymbol.IsStatic )
                {
                    return IdentifierName( targetSymbol.Name );
                }
                else
                {
                    return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) );
                }
            }
        }

        private static EventDeclarationSyntax GetTrampolineEvent( EventFieldDeclarationSyntax eventField, IEventSymbol targetSymbol )
        {
            // TODO: Do not copy leading/trailing trivia to all declarations.

            return
                EventDeclaration(
                        List<AttributeListSyntax>(),
                        eventField.Modifiers,
                        Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( ElasticSpace ),
                        eventField.Declaration.Type,
                        null,
                        eventField.Declaration.Variables.Single().Identifier,
                        AccessorList(
                            List(
                                new[]
                                    {
                                        AccessorDeclaration(
                                                SyntaxKind.AddAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.AddAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace(),
                                        AccessorDeclaration(
                                                SyntaxKind.RemoveAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SubtractAssignmentExpression,
                                                            GetInvocationTarget(),
                                                            IdentifierName( "value" ) ) ) ) )
                                            .NormalizeWhitespace()
                                    }.Where( a => a != null )
                                    .AssertNoneNull() ) ),
                        default )
                    .WithLeadingTrivia( eventField.GetLeadingTrivia() )
                    .WithTrailingTrivia( eventField.GetTrailingTrivia() );

            ExpressionSyntax GetInvocationTarget()
            {
                if ( targetSymbol.IsStatic )
                {
                    return IdentifierName( targetSymbol.Name );
                }
                else
                {
                    return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( targetSymbol.Name ) );
                }
            }
        }
    }
}