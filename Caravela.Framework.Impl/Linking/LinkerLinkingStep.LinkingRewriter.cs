// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
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
                            // Declaration is not inlineable, it will stay but inlineable references need to be inlined into it.
                            newMembers.Add( this.GetTransformedNonInlineableOverride( member, semanticModel ) );
                        }
                    }
                    else if ( this._analysisRegistry.IsInterfaceImplementation( symbol ) )
                    {
                        // Interface implementation member.
                        var interfaceMember = this._analysisRegistry.GetImplementedInterfaceMember( symbol );

                        newMembers.Add( GetTransformedInterfaceImplementation( member, interfaceMember ) );
                    }
                    else if ( this._analysisRegistry.IsOverrideTarget( symbol ) )
                    {
                        // Override target, i.e. original method or introduced method stub.
                        var lastOverrideSymbol = this._analysisRegistry.GetLastOverride( symbol );

                        if ( !this._analysisRegistry.IsInlineable( lastOverrideSymbol ) )
                        {
                            // Body of the last (outermost) override is not inlineable. We need to emit a trampoline method.
                            newMembers.Add( this.GetTransformedInlineableOverrideTarget( member, lastOverrideSymbol ) );
                        }
                        else
                        {
                            // Body of the last (outermost) override is inlineable. We will run inlining on the override's body/bodies and place replace the current body with the result.
                            newMembers.Add( this.GetTransformedNonInlineableOverrideTarget( member, lastOverrideSymbol, semanticModel ) );
                        }

                        // TODO: This should be inserted after all other overrides.
                        if ( !this._analysisRegistry.IsInlineable( symbol ) )
                        {
                            newMembers.Add( GetOriginalImplDeclaration( member ) );
                        }
                        else
                        {
                            if (symbol is IPropertySymbol propertySymbol && IsAutoPropertyDeclaration( (BasePropertyDeclarationSyntax)member ) )
                            {
                                newMembers.Add( GetImplicitBackingFieldDeclaration( (BasePropertyDeclarationSyntax) member, propertySymbol ) );
                            }
                        }
                    }
                    else
                    {
                        // Normal member without any transformations.
                        newMembers.Add( member );
                    }
                }

                return node.WithMembers( List( newMembers ) );
            }

            private MemberDeclarationSyntax GetTransformedNonInlineableOverride( MemberDeclarationSyntax member, SemanticModel semanticModel )
            {
                switch ( member )
                {
                    case MethodDeclarationSyntax method:
                        // Non-inlineable method.
                        return this.TransformMethod( semanticModel, method, method );

                    case PropertyDeclarationSyntax property:
                        // Non-inlineable property.
                        return this.TransformProperty( semanticModel, property, property );

                    case EventDeclarationSyntax @event:
                        // Non-inlineable event.
                        return this.TransformEvent( semanticModel, @event, @event );

                    default:
                        throw new AssertionFailedException();
                }
            }

            private static MemberDeclarationSyntax GetTransformedInterfaceImplementation( MemberDeclarationSyntax member, ISymbol symbol )
            {
                switch ( member )
                {
                    case MethodDeclarationSyntax method:
                        // Non-inlineable method.
                        return TransformInterfaceMethodImplementation( method, (IMethodSymbol) symbol );

                    case PropertyDeclarationSyntax property:
                        // Non-inlineable property.
                        return TransformInterfacePropertyImplementation( property, (IPropertySymbol) symbol );

                    case EventDeclarationSyntax @event:
                        // Non-inlineable event.
                        return TransformInterfaceEventImplementation( @event, (IEventSymbol) symbol );

                    default:
                        throw new AssertionFailedException();
                }
            }

#pragma warning disable CA1822 // Mark members as static
            private MemberDeclarationSyntax GetTransformedInlineableOverrideTarget( MemberDeclarationSyntax member, ISymbol lastOverrideSymbol )
#pragma warning restore CA1822 // Mark members as static
            {
                switch ( member )
                {
                    case MethodDeclarationSyntax method:
                        // Non-inlineable method.
                        return GetTrampolineMethod( method, (IMethodSymbol) lastOverrideSymbol );

                    case PropertyDeclarationSyntax property:
                        // Non-inlineable property.
                        return GetTrampolineProperty( property, (IPropertySymbol) lastOverrideSymbol );

                    case EventDeclarationSyntax @event:
                        // Non-inlineable event.
                        return GetTrampolineEvent( @event, (IEventSymbol) lastOverrideSymbol );

                    default:
                        throw new AssertionFailedException();
                }
            }

            private MemberDeclarationSyntax GetTransformedNonInlineableOverrideTarget(
                MemberDeclarationSyntax member,
                ISymbol lastOverrideSymbol,
                SemanticModel semanticModel )
            {
                switch ( member )
                {
                    case MethodDeclarationSyntax method:
                        var lastMethodOverrideSyntax = (MethodDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                        return this.TransformMethod( semanticModel, method, lastMethodOverrideSyntax );

                    case PropertyDeclarationSyntax property:
                        var lastPropertyOverrideSyntax = (PropertyDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                        return this.TransformProperty( semanticModel, property, lastPropertyOverrideSyntax );

                    case EventDeclarationSyntax @event:
                        var lastEventOverrideSyntax = (EventDeclarationSyntax) lastOverrideSymbol.DeclaringSyntaxReferences.Single().GetSyntax();

                        return this.TransformEvent( semanticModel, @event, lastEventOverrideSyntax );

                    default:
                        throw new AssertionFailedException();
                }
            }

            private static MemberDeclarationSyntax GetOriginalImplDeclaration( MemberDeclarationSyntax member )
            {
                // This is target member that is not inlineable, we need to a separate declaration.
                switch ( member )
                {
                    case MethodDeclarationSyntax method:
                        return GetOriginalImplMethod( method );

                    case PropertyDeclarationSyntax property:
                        return GetOriginalImplProperty( property );

                    case EventDeclarationSyntax @event:
                        return GetOriginalImplEvent( @event );

                    default:
                        throw new AssertionFailedException();
                }
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

                    if ( propertyDeclaration.AccessorList != null )
                    {
                        // Go through accessors on the property and rewrite them.
                        foreach ( var originalAccessor in propertyDeclaration.AccessorList.Accessors )
                        {
                            var accessorBodySource =
                                propertyBodySource.AccessorList.Accessors.SingleOrDefault( a => a.Kind() == originalAccessor.Kind() )
                                ?? originalAccessor;

                            transformedAccessors.Add(
                                AccessorDeclaration(
                                    originalAccessor.Kind(),
                                    this.GetRewrittenPropertyAccessorBody(
                                        semanticModel,
                                        accessorBodySource,
                                        symbol ) ) );
                        }
                    }
                    else
                    {
                        // Expression body property.
                        var accessorBodySource =
                            propertyBodySource.AccessorList.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.GetAccessorDeclaration )
                            ?? AccessorDeclaration( SyntaxKind.GetKeyword, Block( ReturnStatement( propertyDeclaration.ExpressionBody.AssertNotNull().Expression ) ) );

                        transformedAccessors.Add(
                            AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                this.GetRewrittenPropertyAccessorBody(
                                    semanticModel,
                                    accessorBodySource,
                                    symbol ) ) );                         
                    }

                    return propertyDeclaration
                        .WithAccessorList( AccessorList( List( transformedAccessors ) ) )
                        .WithLeadingTrivia( propertyDeclaration.GetLeadingTrivia() )
                        .WithTrailingTrivia( propertyDeclaration.GetTrailingTrivia() )
                        .WithExpressionBody(null)
                        .WithSemicolonToken(Token(SyntaxKind.None));
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

            public static MethodDeclarationSyntax TransformInterfaceMethodImplementation( MethodDeclarationSyntax method, IMethodSymbol interfaceMethodSymbol )
            {
                var interfaceType = interfaceMethodSymbol.ContainingType;

                return
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList(),
                        method.ReturnType,
                        ExplicitInterfaceSpecifier( (NameSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( interfaceType ) ),
                        Identifier( interfaceMethodSymbol.Name ),
                        method.TypeParameterList,
                        method.ParameterList,
                        method.ConstraintClauses,
                        method.Body,
                        null );
            }

            public static PropertyDeclarationSyntax TransformInterfacePropertyImplementation(
                PropertyDeclarationSyntax property,
                IPropertySymbol interfacePropertySymbol )
            {
                var interfaceType = interfacePropertySymbol.ContainingType;

                return
                    PropertyDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList(),
                        property.Type,
                        ExplicitInterfaceSpecifier( (NameSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( interfaceType ) ),
                        Identifier( interfacePropertySymbol.Name ),
                        property.AccessorList.AssertNotNull() );
            }

            public static EventDeclarationSyntax TransformInterfaceEventImplementation( EventDeclarationSyntax @event, IEventSymbol interfaceEventSymbol )
            {
                var interfaceType = interfaceEventSymbol.ContainingType;

                return
                    EventDeclaration(
                        List<AttributeListSyntax>(),
                        TokenList(),
                        @event.Type,
                        ExplicitInterfaceSpecifier( (NameSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( interfaceType ) ),
                        Identifier( interfaceEventSymbol.Name ),
                        @event.AccessorList );
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
                        SyntaxKind.InitAccessorDeclaration => new PropertySetInliningRewriter( this._analysisRegistry, semanticModel, propertySymbol ),
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

            public static string GetImplicitBackingFieldName( IPropertySymbol property )
            {
                return $"__{property.Name}__BackingField";
            }

            private static MemberDeclarationSyntax GetImplicitBackingFieldDeclaration( BasePropertyDeclarationSyntax propertyDeclaration, IPropertySymbol propertySymbol )
            {
                return FieldDeclaration(
                    List<AttributeListSyntax>(),
                    GetModifiers(propertySymbol),
                    VariableDeclaration(
                        propertyDeclaration.Type,
                        SingletonSeparatedList( VariableDeclarator( GetImplicitBackingFieldName( propertySymbol ) ) ) ) );

                static SyntaxTokenList GetModifiers( IPropertySymbol propertySymbol )
                {
                    var modifiers = new List<SyntaxToken>();

                    modifiers.Add( Token( SyntaxKind.PrivateKeyword ) );

                    if ( propertySymbol.IsStatic )
                    {
                        modifiers.Add( Token( SyntaxKind.StaticKeyword ) );
                    }

                    if ( propertySymbol.SetMethod == null )
                    {
                        modifiers.Add( Token( SyntaxKind.ReadOnlyKeyword ) );
                    }

                    return TokenList( modifiers );
                }
            }

            private static bool IsAutoPropertyDeclaration( BasePropertyDeclarationSyntax basePropertyDeclaration )
            {
                switch ( basePropertyDeclaration )
                {
                    case PropertyDeclarationSyntax propertyDeclaration:
                        return propertyDeclaration.ExpressionBody == null
                            && propertyDeclaration.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true
                            && propertyDeclaration.Modifiers.All( x => x.Kind() != SyntaxKind.AbstractKeyword );
                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}