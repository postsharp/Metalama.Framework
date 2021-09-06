// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Caravela.Framework.Tests.Integration.Runners.Linker
{
    internal partial class LinkerTestInputBuilder
    {
        private class TestTypeRewriter : CSharpSyntaxRewriter
        {
            private readonly List<IObservableTransformation> _observableTransformations;
            private readonly List<INonObservableTransformation> _nonObservableTransformations;

            private readonly TestRewriter _owner;
            private readonly Stack<TypeDeclarationSyntax> _currentTypeStack;
            private InsertPosition? _currentInsertPosition;

            public IReadOnlyList<IObservableTransformation> ObservableTransformations => this._observableTransformations;

            public IReadOnlyList<INonObservableTransformation> NonObservableTransformations => this._nonObservableTransformations;

            public TestTypeRewriter( TestRewriter owner )
            {
                this._owner = owner;
                this._currentTypeStack = new Stack<TypeDeclarationSyntax>();
                this._observableTransformations = new List<IObservableTransformation>();
                this._nonObservableTransformations = new List<INonObservableTransformation>();
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                return this.RewriteTypeDeclaration( node, n => base.VisitClassDeclaration( n ) );
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                return this.RewriteTypeDeclaration( node, n => base.VisitRecordDeclaration( n ) );
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                return this.RewriteTypeDeclaration( node, n => base.VisitStructDeclaration( n ) );
            }

            private SyntaxNode? RewriteTypeDeclaration<T>( T node, Func<T, SyntaxNode?> rewriteFunc )
                where T : TypeDeclarationSyntax
            {
                var nodeWithId = AssignNodeId( node );

                this._currentTypeStack.Push( nodeWithId );
                this._currentInsertPosition = new InsertPosition( InsertPositionRelation.Within, nodeWithId );

                var rewrittenNode = rewriteFunc( nodeWithId );

                this._currentTypeStack.Pop();

                if ( this._currentTypeStack.Count > 0 )
                {
                    this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, nodeWithId );
                }
                else
                {
                    this._currentInsertPosition = null;
                }

                return rewrittenNode;
            }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                return this.RewriteMemberDeclaration( node );
            }

            public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                return this.RewriteMemberDeclaration( node );
            }

            public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            {
                return this.RewriteMemberDeclaration( node );
            }

            public override SyntaxNode? VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                return this.RewriteMemberDeclaration( node );
            }

            private SyntaxNode? RewriteMemberDeclaration<T>( T node )
                where T : MemberDeclarationSyntax
            {
                if ( HasPseudoAttribute( node ) )
                {
                    var newNode = this.ProcessPseudoAttributeNode( node, out var isPseudoMember );

                    if ( !isPseudoMember )
                    {
                        newNode = AssignNodeId( newNode.AssertNotNull() );
                        this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, (MemberDeclarationSyntax) newNode.AssertNotNull() );
                    }

                    return newNode;
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, node );

                return node;
            }

            private static bool HasPseudoAttribute( MemberDeclarationSyntax node )
            {
                return node.AttributeLists.SelectMany( x => x.Attributes ).Any( x => x.Name.ToString().StartsWith( "Pseudo", StringComparison.Ordinal ) );
            }

            private SyntaxNode? ProcessPseudoAttributeNode( MemberDeclarationSyntax node, out bool isPseudoMember )
            {
                var newAttributeLists = new List<AttributeListSyntax>();
                AttributeSyntax? pseudoIntroductionAttribute = null;
                AttributeSyntax? pseudoOverrideAttribute = null;

                var notInlineable = false;
                var notDiscardable = false;

                foreach ( var attributeList in node.AttributeLists )
                {
                    // First pass: option attributes
                    foreach ( var attribute in attributeList.Attributes )
                    {
                        var name = attribute.Name.ToString();

                        switch (name)
                        {
                            case "PseudoNotInlineable":
                                notInlineable = true;
                                
                                break;

                            case "PseudoNotDiscardable":
                                notDiscardable = true;
                                
                                break;
                        }
                    }

                    // Second pass: transformation attributes
                    foreach ( var attribute in attributeList.Attributes )
                    {
                        var name = attribute.Name.ToString();

                        if ( string.Equals( name, "PseudoIntroduction", StringComparison.Ordinal ) )
                        {
                            pseudoIntroductionAttribute = attribute;
                        }
                        else if ( string.Equals( name, "PseudoOverride", StringComparison.Ordinal ) )
                        {
                            pseudoOverrideAttribute = attribute;
                        }
                        else if ( name.StartsWith( "Pseudo", StringComparison.Ordinal ) &&
                                  !string.Equals( name, "PseudoNotInlineable", StringComparison.Ordinal ) &&
                                  !string.Equals( name, "PseudoNotDiscardable", StringComparison.Ordinal ) )
                        {
                            throw new NotSupportedException( $"Unsupported pseudo attribute {name}" );
                        }
                    }
                }

                if ( pseudoIntroductionAttribute != null )
                {
                    // Introduction will create a temporary declaration, that will help us to provide values for IMethod member.
                    isPseudoMember = true;

                    return this.ProcessPseudoIntroduction( node, newAttributeLists, pseudoIntroductionAttribute, notInlineable, notDiscardable );
                }
                else if ( pseudoOverrideAttribute != null )
                {
                    isPseudoMember = true;

                    return this.ProcessPseudoOverride( node, newAttributeLists, pseudoOverrideAttribute, notInlineable, notDiscardable );
                }
                else
                {
                    isPseudoMember = false;

                    var transformedNode = node.WithAttributeLists( List( newAttributeLists ) );

                    if ( notInlineable || notDiscardable )
                    {
                        var flags = LinkerDeclarationFlags.None;

                        if ( notInlineable )
                        {
                            flags |= LinkerDeclarationFlags.NotInlineable;
                        }

                        if ( notDiscardable )
                        {
                            flags |= LinkerDeclarationFlags.NotDiscardable;
                        }

                        transformedNode = transformedNode.WithLinkerDeclarationFlags( flags );
                    }

                    return transformedNode;
                }
            }

            private MemberDeclarationSyntax ProcessPseudoIntroduction(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax attribute,
                bool notInlineable,
                bool notDiscardable )
            {
                if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count < 1 || attribute.ArgumentList.Arguments.Count > 3 )
                {
                    throw new ArgumentException( "PseudoIntroduction should have 1 or 2 arguments - aspect name and optionally layer name." );
                }

                var aspectName = attribute.ArgumentList.Arguments[0].ToString().Trim( '\"' );

                string? layerName = null;

                if ( attribute.ArgumentList.Arguments.Count == 2 )
                {
                    layerName = attribute.ArgumentList.Arguments[1].ToString().Trim( '\"' );
                }

                var aspectLayer = this._owner.GetOrAddAspectLayer( aspectName.AssertNotNull(), layerName );

                var symbolHelperDeclaration = GetSymbolHelperDeclaration( node );

                var introductionSyntax = node.WithAttributeLists( List( newAttributeLists ) );

                if ( notInlineable || notDiscardable )
                {
                    var flags = LinkerDeclarationFlags.None;

                    if ( notInlineable )
                    {
                        flags |= LinkerDeclarationFlags.NotInlineable;
                    }

                    if ( notDiscardable )
                    {
                        flags |= LinkerDeclarationFlags.NotDiscardable;
                    }

                    introductionSyntax = introductionSyntax.WithLinkerDeclarationFlags( flags );
                }

                var declarationKind = node switch
                {
                    MethodDeclarationSyntax => DeclarationKind.Method,
                    PropertyDeclarationSyntax => DeclarationKind.Property,
                    EventDeclarationSyntax => DeclarationKind.Event,
                    EventFieldDeclarationSyntax => DeclarationKind.Event,
                    _ => throw new AssertionFailedException()
                };

                // Create transformation fake.
                var transformation = (IMemberIntroduction) A.Fake<object>(
                    o =>
                    {
                        o = o
                            .Implements<IObservableTransformation>()
                            .Implements<IMemberIntroduction>()
                            .Implements<IDeclarationBuilder>()
                            .Implements<IDeclarationInternal>()
                            .Implements<ITestTransformation>();

                        _ = declarationKind switch
                        {
                            DeclarationKind.Method => o.Implements<IMethod>(),
                            DeclarationKind.Property => o.Implements<IProperty>(),
                            DeclarationKind.Event => o.Implements<IEvent>(),
                            _ => throw new AssertionFailedException()
                        };
                    } );

                A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
                A.CallTo( () => transformation.ToString() ).Returns( "Introduced" );
                A.CallTo( () => transformation.TargetSyntaxTree ).Returns( node.SyntaxTree );

                var advice = this.CreateFakeAdvice( aspectLayer );
                A.CallTo( () => transformation.Advice ).Returns( advice );

                A.CallTo( () => transformation.GetIntroducedMembers( A<MemberIntroductionContext>.Ignored ) )
                    .Returns(
                        new[]
                        {
                            new IntroducedMember(
                                transformation,
                                introductionSyntax,
                                new AspectLayerId( aspectName.AssertNotNull(), layerName ),
                                IntroducedMemberSemantic.Introduction,
                                null )
                        } );

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentTypeStack.Peek() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( this._currentInsertPosition!.Value.SyntaxNode != null ? GetNodeId( this._currentInsertPosition.Value.SyntaxNode ) : null );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionBuilder )
                    .Returns( this._currentInsertPosition!.Value.Builder );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionRelation ).Returns( this._currentInsertPosition.Value.Relation );

                var introducedElementName = node switch
                {
                    MethodDeclarationSyntax method => method.Identifier.ValueText,
                    PropertyDeclarationSyntax property => property.Identifier.ValueText,
                    EventDeclarationSyntax @event => @event.Identifier.ValueText,
                    EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables.Single().Identifier.ValueText,
                    _ => throw new NotSupportedException()
                };

                A.CallTo( () => ((ITestTransformation) transformation).IntroducedElementName ).Returns( introducedElementName );

                var symbolHelperId = GetNodeId( symbolHelperDeclaration );
                this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, (IDeclarationBuilder) transformation );
                A.CallTo( () => ((ITestTransformation) transformation).SymbolHelperNodeId ).Returns( symbolHelperId );

                this._observableTransformations.Add( (IObservableTransformation) transformation );

                return symbolHelperDeclaration;
            }

            private MemberDeclarationSyntax ProcessPseudoOverride(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax attribute,
                bool notInlineable,
                bool notDiscardable )
            {
                if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count < 2 || attribute.ArgumentList.Arguments.Count > 3 )
                {
                    throw new ArgumentException(
                        "PseudoOverride should have 2 or 3 arguments - overridden declaration name, aspect name and optionally layer name." );
                }

                var overriddenDeclarationName =
                    ((InvocationExpressionSyntax) attribute.ArgumentList.Arguments[0].Expression).ArgumentList.Arguments[0].ToString();

                var aspectName = attribute.ArgumentList.Arguments[1].ToString().Trim( '\"' );

                string? layerName = null;

                if ( attribute.ArgumentList.Arguments.Count == 3 )
                {
                    layerName = attribute.ArgumentList.Arguments[2].ToString().Trim( '\"' );
                }

                var aspectLayer = this._owner.GetOrAddAspectLayer( aspectName.AssertNotNull(), layerName );

                var transformation = (IMemberIntroduction) A.Fake<object>(
                    o => o
                        .Implements<INonObservableTransformation>()
                        .Implements<IMemberIntroduction>()
                        .Implements<IOverriddenDeclaration>()
                        .Implements<ITestTransformation>() );

                var methodBodyRewriter = new TestMethodBodyRewriter( aspectName, layerName );
                MemberDeclarationSyntax overrideSyntax;

                switch ( node )
                {
                    case MethodDeclarationSyntax { Body: not null } method:
                        var rewrittenMethodBody = methodBodyRewriter.VisitBlock( method.Body.AssertNotNull() );

                        overrideSyntax =
                            method
                                .WithAttributeLists( List( newAttributeLists ) )
                                .WithBody( (BlockSyntax) rewrittenMethodBody.AssertNotNull() );

                        break;

                    case MethodDeclarationSyntax { ExpressionBody: not null } method:
                        var rewrittenMethodExpressionBody = methodBodyRewriter.VisitArrowExpressionClause( method.ExpressionBody.AssertNotNull() );

                        overrideSyntax =
                            method
                                .WithAttributeLists( List( newAttributeLists ) )
                                .WithExpressionBody( (ArrowExpressionClauseSyntax) rewrittenMethodExpressionBody.AssertNotNull() );

                        break;

                    case PropertyDeclarationSyntax { AccessorList: not null } property when property.AccessorList!.Accessors.All( a => a.Body != null ):
                        overrideSyntax =
                            property
                                .WithAttributeLists( List( newAttributeLists ) )
                                .WithAccessorList(
                                    AccessorList(
                                        List(
                                            property.AccessorList!.Accessors.Select(
                                                a => a.WithBody( (BlockSyntax) methodBodyRewriter.VisitBlock( a.Body! ).AssertNotNull() ) ) ) ) );

                        break;

                    case EventDeclarationSyntax @event:
                        overrideSyntax =
                            @event
                                .WithAttributeLists( List( newAttributeLists ) )
                                .WithAccessorList(
                                    AccessorList(
                                        List(
                                            @event.AccessorList!.Accessors.Select(
                                                a => a.WithBody( (BlockSyntax) methodBodyRewriter.VisitBlock( a.Body! ).AssertNotNull() ) ) ) ) );

                        break;

                    case EventFieldDeclarationSyntax eventField:
                        overrideSyntax =
                            eventField
                                .WithAttributeLists( List( newAttributeLists ) );

                        break;

                    default:
                        throw new NotSupportedException();
                }

                if ( notInlineable || notDiscardable )
                {
                    var flags = LinkerDeclarationFlags.None;

                    if ( notInlineable )
                    {
                        flags |= LinkerDeclarationFlags.NotInlineable;
                    }

                    if ( notDiscardable )
                    {
                        flags |= LinkerDeclarationFlags.NotDiscardable;
                    }

                    overrideSyntax = overrideSyntax.WithLinkerDeclarationFlags( flags );
                }

                var symbolHelperDeclaration = GetSymbolHelperDeclaration( node );

                A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
                A.CallTo( () => transformation.ToString() ).Returns( "Override" );

                var advice = this.CreateFakeAdvice( aspectLayer );
                A.CallTo( () => transformation.Advice ).Returns( advice );

                A.CallTo( () => transformation.GetIntroducedMembers( A<MemberIntroductionContext>.Ignored ) )
                    .Returns(
                        new[]
                        {
                            new IntroducedMember(
                                transformation,
                                overrideSyntax,
                                new AspectLayerId( aspectName.AssertNotNull(), layerName ),
                                node switch
                                {
                                    MethodDeclarationSyntax _ => IntroducedMemberSemantic.Override,
                                    PropertyDeclarationSyntax _ => IntroducedMemberSemantic.Override,
                                    EventDeclarationSyntax _ => IntroducedMemberSemantic.Override,
                                    EventFieldDeclarationSyntax _ => IntroducedMemberSemantic.Override,
                                    _ => throw new NotSupportedException()
                                },
                                null )
                        } );

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentTypeStack.Peek() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( this._currentInsertPosition!.Value.SyntaxNode != null ? GetNodeId( this._currentInsertPosition.Value.SyntaxNode ) : null );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionBuilder )
                    .Returns( this._currentInsertPosition!.Value.Builder );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionRelation ).Returns( this._currentInsertPosition.Value.Relation );

                A.CallTo( () => ((ITestTransformation) transformation).OverriddenDeclarationName ).Returns( overriddenDeclarationName );
                A.CallTo( () => ((ITestTransformation) transformation).SymbolHelperNodeId ).Returns( GetNodeId( symbolHelperDeclaration ) );

                this._nonObservableTransformations.Add( (INonObservableTransformation) transformation );

                return symbolHelperDeclaration;
            }

            private static MemberDeclarationSyntax GetSymbolHelperDeclaration( MemberDeclarationSyntax node )
            {
                return (MemberDeclarationSyntax) AssignNodeId(
                    MarkTemporary(
                        node switch
                        {
                            FieldDeclarationSyntax field => GetSymbolHelperField( field ),
                            MethodDeclarationSyntax method => GetSymbolHelperMethod( method ),
                            PropertyDeclarationSyntax property => GetSymbolHelperProperty( property ),
                            EventDeclarationSyntax @event => GetSymbolHelperEvent( @event ),
                            EventFieldDeclarationSyntax eventField => GetSymbolHelperEventField( eventField ),
                            _ => throw new NotSupportedException()
                        } ) );
            }

            private static SyntaxNode GetSymbolHelperField( FieldDeclarationSyntax field )
            {
                return field
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithDeclaration(
                        field.Declaration.WithVariables(
                            SeparatedList(
                                field.Declaration.Variables.Select( v => v.WithIdentifier( Identifier( v.Identifier.ValueText + "__SymbolHelper" ) ) ) ) ) );
            }

            private static SyntaxNode GetSymbolHelperMethod( MethodDeclarationSyntax method )
            {
                return method
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithModifiers( TokenList( method.Modifiers.Where( m => m.Kind() != SyntaxKind.OverrideKeyword ) ) )
                    .WithIdentifier( Identifier( method.Identifier.ValueText + "__SymbolHelper" ) )
                    .WithExpressionBody( null )
                    .WithBody(
                        method.ReturnType.ToString() == "void"
                            ? Block()
                            : Block(
                                ReturnStatement(
                                    LiteralExpression(
                                        SyntaxKind.DefaultLiteralExpression,
                                        Token( SyntaxKind.DefaultKeyword ) ) ) ) );
            }

            private static SyntaxNode GetSymbolHelperProperty( PropertyDeclarationSyntax property )
            {
                if ( property.AccessorList != null )
                {
                    return property
                        .WithAttributeLists( List<AttributeListSyntax>() )
                        .WithIdentifier( Identifier( property.Identifier.ValueText + "__SymbolHelper" ) )
                        .WithModifiers( TokenList( property.Modifiers.Where( m => m.Kind() != SyntaxKind.OverrideKeyword ) ) )
                        .WithInitializer( null )
                        .WithAccessorList(
                            AccessorList(
                                List(
                                    property.AccessorList.Accessors.Select(
                                        a => a switch
                                        {
                                            _ when a.Kind() == SyntaxKind.GetAccessorDeclaration =>
                                                a
                                                    .WithExpressionBody( null )
                                                    .WithBody(
                                                        Block(
                                                            ReturnStatement(
                                                                LiteralExpression(
                                                                    SyntaxKind.DefaultLiteralExpression,
                                                                    Token( SyntaxKind.DefaultKeyword ) ) ) ) ),
                                            _ when a.Kind() == SyntaxKind.SetAccessorDeclaration =>
                                                a.WithBody( Block() ),
                                            _ => throw new NotSupportedException()
                                        } ) ) ) );
                }
                else
                {
                    return property
                        .WithAttributeLists( List<AttributeListSyntax>() )
                        .WithIdentifier( Identifier( property.Identifier.ValueText + "__SymbolHelper" ) )
                        .WithModifiers( TokenList( property.Modifiers.Where( m => m.Kind() != SyntaxKind.OverrideKeyword ) ) )
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token( SyntaxKind.DefaultKeyword ) ) ) );
                }
            }

            private static SyntaxNode GetSymbolHelperEvent( EventDeclarationSyntax @event )
            {
                return @event
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithIdentifier( Identifier( @event.Identifier.ValueText + "__SymbolHelper" ) )
                    .WithModifiers( TokenList( @event.Modifiers.Where( m => m.Kind() != SyntaxKind.OverrideKeyword ) ) )
                    .WithAccessorList( AccessorList( List( @event.AccessorList.AssertNotNull().Accessors.Select( a => a.WithBody( Block() ) ) ) ) );
            }

            private static SyntaxNode GetSymbolHelperEventField( EventFieldDeclarationSyntax eventField )
            {
                return eventField
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithDeclaration(
                        eventField.Declaration.WithVariables(
                            SeparatedList(
                                eventField.Declaration.Variables.Select(
                                    v => v.WithIdentifier( Identifier( v.Identifier.ValueText + "__SymbolHelper" ) ) ) ) ) );
            }

            private Advice CreateFakeAdvice( AspectLayerId aspectLayer )
            {
                var fakeAspectSymbol = A.Fake<INamedTypeSymbol>();
                var fakeGlobalNamespaceSymbol = A.Fake<INamespaceSymbol>();
                var fakeDiagnosticAdder = A.Fake<IDiagnosticAdder>();
                var fakeCompilation = A.Fake<Compilation>();

                A.CallTo( () => fakeAspectSymbol.MetadataName ).Returns( aspectLayer.AspectName.AssertNotNull() );
                A.CallTo( () => fakeAspectSymbol.ContainingSymbol ).Returns( fakeGlobalNamespaceSymbol );
                A.CallTo( () => fakeAspectSymbol.DeclaringSyntaxReferences ).Returns( ImmutableArray<SyntaxReference>.Empty );
                A.CallTo( () => fakeGlobalNamespaceSymbol.IsGlobalNamespace ).Returns( true );

                var aspectClass =
                    new AspectClass(
                        this._owner.ServiceProvider,
                        fakeAspectSymbol,
                        null,
                        null,
                        typeof(object),
                        null,
                        fakeDiagnosticAdder,
                        null!,
                        new AspectDriverFactory( this._owner.ServiceProvider, fakeCompilation, ImmutableArray<object>.Empty ) );

                var fakeAspectInstance = new AspectInstance( A.Fake<IAspect>(), A.Fake<IDeclaration>(), aspectClass );

                return A.Fake<Advice>( i => i.WithArgumentsForConstructor( new object?[] { fakeAspectInstance, aspectLayer, null } ) );
            }
        }
    }
}