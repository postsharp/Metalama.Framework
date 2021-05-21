// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        /// <summary>
        /// Rewriter which rewrites classes and methods producing the linked and inlined syntax tree.
        /// </summary>
        private class LinkingRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation _intermediateCompilation;
            private readonly LinkerAnalysisRegistry _analysisRegistry;

            public LinkingRewriter(
                Compilation intermediateCompilation,
                LinkerAnalysisRegistry referenceRegistry )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._analysisRegistry = referenceRegistry;
            }

            internal static string GetOriginalImplMemberName( string memberName ) => $"__{memberName}__OriginalImpl";

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                // TODO: Other transformations than method overrides.
                var newMembers = new List<MemberDeclarationSyntax>();

                foreach ( var member in node.Members )
                {
                    // Go through all members of the type.
                    // For members that represent overrides:
                    //  * If the member can be inlined, skip it.
                    //  * If the member cannot be inlined (or is the root of inlining), add the transformed member with all possible inlining instances.
                    // For members that represent override targets (i.e. overridden members):
                    //  * If the last (transformation order) override is inlineable, replace the member with it's transformed body.
                    //  * Otherwise create a stub that calls the last override.

                    var semanticModel = this._intermediateCompilation.GetSemanticModel( node.SyntaxTree );
                    var symbol = semanticModel.GetDeclaredSymbol( member );

                    if ( symbol == null )
                    {
                        newMembers.Add( member );

                        continue;
                    }

                    if ( this._analysisRegistry.IsOverride( symbol ) )
                    {
                        // Override of a declaration.

                        if ( this._analysisRegistry.IsInlineable( symbol ) )
                        {
                            // Declaration is inlineable, it can be removed as it's body/bodies will be inlined.
                        }
                        else
                        {
                            switch ( member )
                            {
                                case MethodDeclarationSyntax method:
                                    // Non-inlineable method.
                                    var transformedMethod = this.TransformMethod( semanticModel, method, method );
                                    newMembers.Add( transformedMethod );

                                    break;

                                case PropertyDeclarationSyntax property:
                                    // Non-inlineable property.
                                    var transformedProperty = this.TransformProperty( semanticModel, property, property );
                                    newMembers.Add( transformedProperty );

                                    break;

                                case EventDeclarationSyntax @event:
                                    // Non-inlineable event.
                                    var transformedEvent = this.TransformEvent( semanticModel, @event, @event );
                                    newMembers.Add( transformedEvent );

                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }
                        }
                    }
                    else if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
                    {
                        // Override target, i.e. original method or introduced method stub.
                        var lastOverrideSymbol = this._analysisRegistry.GetLastOverride( symbol );

                        if ( !this._analysisRegistry.IsInlineable( lastOverrideSymbol ) )
                        {
                            // Body of the last (outermost) override is not inlineable. We need to emit a trampoline method.
                            switch ( member )
                            {
                                case MethodDeclarationSyntax method:
                                    var transformedMethod = GetTrampolineMethod( method, (IMethodSymbol) lastOverrideSymbol );
                                    newMembers.Add( transformedMethod );

                                    break;

                                case PropertyDeclarationSyntax property:
                                    var transformedProperty = GetTrampolineProperty( property, (IPropertySymbol) lastOverrideSymbol );
                                    newMembers.Add( transformedProperty );

                                    break;

                                case EventDeclarationSyntax @event:
                                    var transformedEvent = GetTrampolineEvent( @event, (IEventSymbol) lastOverrideSymbol );
                                    newMembers.Add( transformedEvent );

                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }
                        }
                        else
                        {
                            // Body of the last (outermost) override is inlineable. We will run inlining on the override's body/bodies and place replace the current body with the result.
                            switch ( member )
                            {
                                case MethodDeclarationSyntax method:
                                    var lastMethodOverrideSyntax = (MethodDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();
                                    var transformedMethod = this.TransformMethod( semanticModel, method, lastMethodOverrideSyntax );
                                    newMembers.Add( transformedMethod );

                                    break;

                                case PropertyDeclarationSyntax property:
                                    var lastPropertyOverrideSyntax =
                                        (PropertyDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                                    var transformedProperty = this.TransformProperty( semanticModel, property, lastPropertyOverrideSyntax );
                                    newMembers.Add( transformedProperty );

                                    break;

                                case EventDeclarationSyntax @event:
                                    var lastEventOverrideSyntax = (EventDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();
                                    var transformedEvent = this.TransformEvent( semanticModel, @event, lastEventOverrideSyntax );
                                    newMembers.Add( transformedEvent );

                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }
                        }

                        if ( !this._analysisRegistry.IsInlineable( symbol ) )
                        {
                            // TODO: This should be inserted after all other overrides.

                            // This is target member that is not inlineable, we need to a separate declaration.
                            switch ( member )
                            {
                                case MethodDeclarationSyntax method:
                                    var originalBodyMethod = GetOriginalImplMethod( method );
                                    newMembers.Add( originalBodyMethod );

                                    break;

                                case PropertyDeclarationSyntax property:
                                    var originalBodyProperty = GetOriginalImplProperty( property );
                                    newMembers.Add( originalBodyProperty );

                                    break;

                                case EventDeclarationSyntax @event:
                                    var originalBodyEvent = GetOriginalImplEvent( @event );
                                    newMembers.Add( originalBodyEvent );

                                    break;

                                default:
                                    throw new AssertionFailedException();
                            }
                        }
                    }
                    else
                    {
                        // Normal method without any transformations.
                        newMembers.Add( member );
                    }
                }

                return node.WithMembers( List( newMembers ) );
            }

            private MethodDeclarationSyntax TransformMethod(
                SemanticModel semanticModel,
                MethodDeclarationSyntax methodDeclaration,
                MethodDeclarationSyntax methodBodySource )
            {
                var symbol = semanticModel.GetDeclaredSymbol( methodDeclaration ).AssertNotNull();

                return
                    methodDeclaration.WithBody( this.GetRewrittenMethodBody( semanticModel, methodBodySource, symbol ) )
                        .WithLeadingTrivia( methodDeclaration.GetLeadingTrivia() )
                        .WithTrailingTrivia( methodDeclaration.GetTrailingTrivia() );
            }

            private PropertyDeclarationSyntax TransformProperty(
                SemanticModel semanticModel,
                PropertyDeclarationSyntax propertyDeclaration,
                PropertyDeclarationSyntax propertyBodySource )
            {
                if ( propertyBodySource.AccessorList != null )
                {
                    var transformedAccessors = new List<AccessorDeclarationSyntax>();
                    var symbol = semanticModel.GetDeclaredSymbol( propertyDeclaration ).AssertNotNull();

                    foreach ( var originalAccessor in propertyBodySource.AccessorList.Accessors )
                    {
                        transformedAccessors.Add(
                            AccessorDeclaration(
                                originalAccessor.Kind(),
                                this.GetRewrittenPropertyAccessorBody(
                                    semanticModel,
                                    originalAccessor,
                                    symbol ) ) );
                    }

                    return propertyDeclaration
                        .WithAccessorList( AccessorList( List( transformedAccessors ) ) )
                        .WithLeadingTrivia( propertyDeclaration.GetLeadingTrivia() )
                        .WithTrailingTrivia( propertyDeclaration.GetTrailingTrivia() );
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            private EventDeclarationSyntax TransformEvent(
                SemanticModel semanticModel,
                EventDeclarationSyntax eventDeclaration,
                EventDeclarationSyntax propertyBodySource )
            {
                if ( propertyBodySource.AccessorList != null )
                {
                    var transformedAccessors = new List<AccessorDeclarationSyntax>();

                    foreach ( var originalAccessor in propertyBodySource.AccessorList.Accessors )
                    {
                        var symbol = semanticModel.GetDeclaredSymbol( originalAccessor ).AssertNotNull();

                        transformedAccessors.Add(
                            AccessorDeclaration(
                                originalAccessor.Kind(),
                                this.GetRewrittenEventAccessorBody(
                                    semanticModel,
                                    originalAccessor,
                                    symbol ) ) );
                    }

                    return eventDeclaration
                        .WithAccessorList( AccessorList( List( transformedAccessors ) ) )
                        .WithLeadingTrivia( eventDeclaration.GetLeadingTrivia() )
                        .WithTrailingTrivia( eventDeclaration.GetTrailingTrivia() );
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            private static MethodDeclarationSyntax GetTrampolineMethod( MethodDeclarationSyntax method, IMethodSymbol targetSymbol )
            {
                // TODO: First override not being inlineable probably does not happen outside of specifically written linker tests, i.e. trampolines may not be needed.

                return method
                    .WithBody( GetBody() )
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
                        return Block( ReturnStatement( invocation ) );
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
                                                Block( ReturnStatement( GetInvocationTarget() ) ) )
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
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                GetInvocationTarget(),
                                                                IdentifierName( "e" ) ),
                                                            IdentifierName( "value" ) ) ) ) )
                                            : null,
                                        removeAccessor != null
                                            ? AccessorDeclaration(
                                                SyntaxKind.AddAccessorDeclaration,
                                                Block(
                                                    ExpressionStatement(
                                                        AssignmentExpression(
                                                            SyntaxKind.SubtractAssignmentExpression,
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                GetInvocationTarget(),
                                                                IdentifierName( "e" ) ),
                                                            IdentifierName( "value" ) ) ) ) )
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

            private BlockSyntax GetRewrittenMethodBody( SemanticModel semanticModel, MethodDeclarationSyntax method, IMethodSymbol symbol )
            {
                // Create inlining rewriter and inline calls into this method's body.
                var inliningRewriter = new MethodInliningRewriter( this._analysisRegistry, semanticModel, symbol );

                if ( method.Body != null )
                {
                    return (BlockSyntax) inliningRewriter.VisitBlock( method.Body ).AssertNotNull();
                }
                else if ( method.ExpressionBody != null )
                {
                    // TODO: Correct trivia for the generated block body.
                    return (BlockSyntax) inliningRewriter.VisitBlock( Block( ExpressionStatement( method.ExpressionBody.Expression ) ) ).AssertNotNull();
                }
                else
                {
                    throw new AssertionFailedException( $"{method}" );
                }
            }

            private BlockSyntax GetRewrittenPropertyAccessorBody(
                SemanticModel semanticModel,
                AccessorDeclarationSyntax accessor,
                IPropertySymbol propertySymbol )
            {
                // Create inlining rewriter and inline calls into this method's body.
                InliningRewriterBase inliningRewriter =
                    accessor.Kind() switch
                    {
                        SyntaxKind.GetAccessorDeclaration => new PropertyGetInliningRewriter( this._analysisRegistry, semanticModel, propertySymbol ),
                        SyntaxKind.SetAccessorDeclaration => new PropertySetInliningRewriter( this._analysisRegistry, semanticModel, propertySymbol ),
                        _ => throw new AssertionFailedException( $"{accessor.Kind()}" )
                    };

                if ( accessor.Body != null )
                {
                    return (BlockSyntax) inliningRewriter.VisitBlock( accessor.Body ).AssertNotNull();
                }
                else if ( accessor.ExpressionBody != null )
                {
                    // TODO: Correct trivia for the generated block body.
                    return (BlockSyntax) inliningRewriter.VisitBlock( Block( ExpressionStatement( accessor.ExpressionBody.Expression ) ) ).AssertNotNull();
                }
                else
                {
                    throw new AssertionFailedException( $"{accessor}" );
                }
            }

            private BlockSyntax GetRewrittenEventAccessorBody( SemanticModel semanticModel, AccessorDeclarationSyntax accessor, IMethodSymbol symbol )
            {
                // Turn off warnings.
                _ = this;
                _ = semanticModel;
                _ = accessor;
                _ = symbol;
                
                throw new NotImplementedException();
            }

            private static MemberDeclarationSyntax GetOriginalImplMethod( MethodDeclarationSyntax method )
            {
                return method.WithIdentifier( Identifier( GetOriginalImplMemberName( method.Identifier.ValueText ) ) );
            }

            private static MemberDeclarationSyntax GetOriginalImplProperty( PropertyDeclarationSyntax property )
            {
                return property.WithIdentifier( Identifier( GetOriginalImplMemberName( property.Identifier.ValueText ) ) );
            }

            private static MemberDeclarationSyntax GetOriginalImplEvent( EventDeclarationSyntax @event )
            {
                return @event.WithIdentifier( Identifier( GetOriginalImplMemberName( @event.Identifier.ValueText ) ) );
            }
        }
    }
}