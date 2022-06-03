﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using FakeItEasy;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Metalama.Framework.Tests.Integration.Runners.Linker
{
    internal partial class LinkerTestInputBuilder
    {
        private class TestTypeRewriter : CSharpSyntaxRewriter
        {
            private readonly List<IObservableTransformation> _observableTransformations;
            private readonly List<IObservableTransformation> _replacedTransformations;
            private readonly List<INonObservableTransformation> _nonObservableTransformations;

            private readonly TestRewriter _owner;
            private readonly Stack<(TypeDeclarationSyntax Type, List<MemberDeclarationSyntax> Members)> _currentTypeStack;
            private InsertPosition? _currentInsertPosition;

            public IReadOnlyList<IObservableTransformation> ObservableTransformations => this._observableTransformations;

            public IReadOnlyList<IObservableTransformation> ReplacedTransformations => this._replacedTransformations;

            public IReadOnlyList<INonObservableTransformation> NonObservableTransformations => this._nonObservableTransformations;

            public TestTypeRewriter( TestRewriter owner )
            {
                this._owner = owner;
                this._currentTypeStack = new Stack<(TypeDeclarationSyntax, List<MemberDeclarationSyntax>)>();
                this._observableTransformations = new List<IObservableTransformation>();
                this._replacedTransformations = new List<IObservableTransformation>();
                this._nonObservableTransformations = new List<INonObservableTransformation>();
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var rewrittenNode = this.RewriteTypeDeclaration( node, n => base.VisitClassDeclaration( n ), ( n, m ) => n.WithMembers( List( m ) ) );

                if ( this._currentTypeStack.Count > 0 )
                {
                    this._currentTypeStack.Peek().Members.Add( rewrittenNode );

                    return null;
                }
                else
                {
                    return rewrittenNode;
                }
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                var rewrittenNode = this.RewriteTypeDeclaration( node, n => base.VisitRecordDeclaration( n ), ( n, m ) => n.WithMembers( List( m ) ) );

                if ( this._currentTypeStack.Count > 0 )
                {
                    this._currentTypeStack.Peek().Members.Add( rewrittenNode );

                    return null;
                }
                else
                {
                    return rewrittenNode;
                }
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                var rewrittenNode = this.RewriteTypeDeclaration( node, n => base.VisitStructDeclaration( n ), ( n, m ) => n.WithMembers( List( m ) ) );

                if ( this._currentTypeStack.Count > 0 )
                {
                    this._currentTypeStack.Peek().Members.Add( rewrittenNode );

                    return null;
                }
                else
                {
                    return rewrittenNode;
                }
            }

            private T RewriteTypeDeclaration<T>( T node, Action<T> visitAction, Func<T, List<MemberDeclarationSyntax>, T> rewriteFunc )
                where T : TypeDeclarationSyntax
            {
                node = AssignNodeId( node );

                var newMemberList = new List<MemberDeclarationSyntax>();

                this._currentTypeStack.Push( (node, newMemberList) );
                this._currentInsertPosition = new InsertPosition( InsertPositionRelation.Within, node );

                visitAction( node );
                var rewrittenNode = rewriteFunc( node, newMemberList );

                this._currentTypeStack.Pop();

                if ( this._currentTypeStack.Count > 0 )
                {
                    this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, node );
                }
                else
                {
                    this._currentInsertPosition = null;
                }

                return rewrittenNode;
            }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                this._currentTypeStack.Peek().Members.AddRange( this.RewriteMemberDeclaration( node ) );

                return null;
            }

            public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                this._currentTypeStack.Peek().Members.AddRange( this.RewriteMemberDeclaration( node ) );

                return null;
            }

            public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            {
                this._currentTypeStack.Peek().Members.AddRange( this.RewriteMemberDeclaration( node ) );

                return null;
            }

            public override SyntaxNode? VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                this._currentTypeStack.Peek().Members.AddRange( this.RewriteMemberDeclaration( node ) );

                return null;
            }

            public override SyntaxNode? VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                this._currentTypeStack.Peek().Members.AddRange( this.RewriteMemberDeclaration( node ) );

                return null;
            }

            private MemberDeclarationSyntax[] RewriteMemberDeclaration<T>( T node )
                where T : MemberDeclarationSyntax
            {
                if ( HasPseudoAttribute( node ) )
                {
                    var newMembers = this.ProcessPseudoAttributeNode( node );
                    var newMemberList = new List<MemberDeclarationSyntax>();

                    foreach ( var newMember in newMembers )
                    {
                        if ( !newMember.IsPseudoMember )
                        {
                            var nodeWithId = AssignNodeId( newMember.Node.AssertNotNull() );
                            newMemberList.Add( nodeWithId );
                            this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, nodeWithId );
                        }
                        else
                        {
                            newMemberList.Add( newMember.Node );
                        }
                    }

                    return newMemberList.ToArray();
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = new InsertPosition( InsertPositionRelation.After, node );

                return new MemberDeclarationSyntax[] { node };
            }

            private static bool HasPseudoAttribute( MemberDeclarationSyntax node )
            {
                return node.AttributeLists.SelectMany( x => x.Attributes ).Any( x => x.Name.ToString().StartsWith( "Pseudo", StringComparison.Ordinal ) );
            }

            private (MemberDeclarationSyntax Node, bool IsPseudoMember)[] ProcessPseudoAttributeNode( MemberDeclarationSyntax node )
            {
                var newAttributeLists = new List<AttributeListSyntax>();
                AttributeSyntax? pseudoIntroductionAttribute = null;
                AttributeSyntax? pseudoOverrideAttribute = null;
                AttributeSyntax? pseudoReplacedAttribute = null;
                AttributeSyntax? pseudoReplacementAttribute = null;

                var notInlineable = false;
                var notDiscardable = false;

                foreach ( var attributeList in node.AttributeLists )
                {
                    // First pass: option attributes
                    foreach ( var attribute in attributeList.Attributes )
                    {
                        var name = attribute.Name.ToString();

                        switch ( name )
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
                        else if ( string.Equals( name, "PseudoReplaced", StringComparison.Ordinal ) )
                        {
                            pseudoReplacedAttribute = attribute;
                        }
                        else if ( string.Equals( name, "PseudoReplacement", StringComparison.Ordinal ) )
                        {
                            pseudoReplacementAttribute = attribute;
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
                    return new[]
                    {
                        (this.ProcessPseudoIntroduction( node, newAttributeLists, pseudoIntroductionAttribute, notInlineable, notDiscardable, pseudoReplacedAttribute, pseudoReplacementAttribute ),
                         true)
                    };
                }
                else if ( pseudoOverrideAttribute != null )
                {
                    Invariant.Assert( pseudoReplacedAttribute == null && pseudoReplacementAttribute == null );

                    return new[] { (this.ProcessPseudoOverride( node, newAttributeLists, pseudoOverrideAttribute, notInlineable, notDiscardable ), true) };
                }
                else if ( pseudoReplacedAttribute != null )
                {
                    var replacedMemberName = node switch
                    {
                        MethodDeclarationSyntax method => method.Identifier.ValueText,
                        PropertyDeclarationSyntax property => property.Identifier.ValueText,
                        EventDeclarationSyntax @event => @event.Identifier.ValueText,
                        EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables.Single().Identifier.ValueText,
                        FieldDeclarationSyntax field => field.Declaration.Variables.Single().Identifier.ValueText,
                        _ => throw new NotSupportedException()
                    };

                    var symbolHelper = GetSymbolHelperDeclaration( node, GetReplacedMemberName( replacedMemberName ) );

                    return new[] { (node.WithAttributeLists( List( newAttributeLists ) ), false), (symbolHelper, true) };
                }
                else
                {
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

                    return new[] { (transformedNode, false) };
                }
            }

            private MemberDeclarationSyntax ProcessPseudoIntroduction(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax introductionAttribute,
                bool notInlineable,
                bool notDiscardable,
                AttributeSyntax? replacedAttribute,
                AttributeSyntax? replacementAttribute )
            {
                if ( introductionAttribute.ArgumentList == null || introductionAttribute.ArgumentList.Arguments.Count < 1
                                                                || introductionAttribute.ArgumentList.Arguments.Count > 3 )
                {
                    throw new ArgumentException( "PseudoIntroduction should have 1 or 2 arguments - aspect name and optionally layer name." );
                }

                var aspectName = introductionAttribute.ArgumentList.Arguments[0].ToString().Trim( '\"' );

                string? layerName = null;

                if ( introductionAttribute.ArgumentList.Arguments.Count == 2 )
                {
                    layerName = introductionAttribute.ArgumentList.Arguments[1].ToString().Trim( '\"' );
                }

                var introducedElementName = node switch
                {
                    MethodDeclarationSyntax method => method.Identifier.ValueText,
                    PropertyDeclarationSyntax property => property.Identifier.ValueText,
                    EventDeclarationSyntax @event => @event.Identifier.ValueText,
                    EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables.Single().Identifier.ValueText,
                    FieldDeclarationSyntax field => field.Declaration.Variables.Single().Identifier.ValueText,
                    _ => throw new NotSupportedException()
                };

                var introductionSyntax = node.WithAttributeLists( List( newAttributeLists ) );

                string? memberNameOverride = null;

                if ( replacementAttribute != null )
                {
                    Invariant.Assert( replacedAttribute == null );
                    Invariant.Assert( replacementAttribute.ArgumentList?.Arguments.Count == 1 );

                    memberNameOverride = ((InvocationExpressionSyntax) replacementAttribute.ArgumentList.Arguments[0].Expression).ArgumentList.Arguments[0]
                        .ToString();

                    introductionSyntax = GetFinalIntroductionSyntax( introductionSyntax, memberNameOverride );
                }
                else if ( replacedAttribute != null )
                {
                    Invariant.Assert( replacementAttribute == null );
                    memberNameOverride = GetReplacedMemberName( introducedElementName );
                }

                var aspectLayer = this._owner.GetOrAddAspectLayer( aspectName.AssertNotNull(), layerName );

                var symbolHelperDeclaration = GetSymbolHelperDeclaration( node, memberNameOverride );

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
                    FieldDeclarationSyntax => DeclarationKind.Field,
                    _ => throw new AssertionFailedException()
                };

                // Create transformation fake.
                var transformation = (IIntroduceMemberTransformation) A.Fake<object>(
                    o =>
                    {
                        _ = o
                            .Implements<IObservableTransformation>()
                            .Implements<IIntroduceMemberTransformation>()
                            .Implements<IMemberBuilder>()
                            .Implements<IDeclarationImpl>()
                            .Implements<ITestTransformation>();

                        _ = declarationKind switch
                        {
                            DeclarationKind.Method => o.Implements<IMethod>().Implements<IRefImpl<IMethod>>(),
                            DeclarationKind.Property => o.Implements<IProperty>().Implements<IRefImpl<IProperty>>(),
                            DeclarationKind.Event => o.Implements<IEvent>().Implements<IRefImpl<IEvent>>(),
                            DeclarationKind.Field => o.Implements<IField>().Implements<IRefImpl<IField>>(),
                            _ => throw new AssertionFailedException()
                        };

                        if ( replacementAttribute != null )
                        {
                            _ = o.Implements<IReplaceMemberTransformation>();
                        }
                    } );

                switch ( declarationKind )
                {
                    case DeclarationKind.Method:
                        A.CallTo( () => ((IRefImpl<IMethod>) transformation).Target ).Returns( transformation );

                        A.CallTo( () => ((IRefImpl<IMethod>) transformation).GetTarget( A<CompilationModel>.Ignored ) )
                            .Returns( (IMethod) transformation );

                        break;

                    case DeclarationKind.Property:
                        A.CallTo( () => ((IRefImpl<IProperty>) transformation).Target ).Returns( transformation );

                        A.CallTo( () => ((IRefImpl<IProperty>) transformation).GetTarget( A<CompilationModel>.Ignored ) )
                            .Returns( (IProperty) transformation );

                        break;

                    case DeclarationKind.Event:
                        A.CallTo( () => ((IRefImpl<IEvent>) transformation).Target ).Returns( transformation );
                        A.CallTo( () => ((IRefImpl<IEvent>) transformation).GetTarget( A<CompilationModel>.Ignored ) ).Returns( (IEvent) transformation );

                        break;

                    case DeclarationKind.Field:
                        A.CallTo( () => ((IRefImpl<IField>) transformation).Target ).Returns( transformation );
                        A.CallTo( () => ((IRefImpl<IField>) transformation).GetTarget( A<CompilationModel>.Ignored ) ).Returns( (IField) transformation );

                        break;
                }

                A.CallTo( () => ((ISdkDeclaration) transformation).Symbol ).Returns( null );
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
                                declarationKind,
                                introductionSyntax,
                                new AspectLayerId( aspectName.AssertNotNull(), layerName ),
                                IntroducedMemberSemantic.Introduction,
                                null )
                        } );

                if ( memberNameOverride != null )
                {
                    A.CallTo( () => ((ITestTransformation) transformation).ReplacedElementName ).Returns( memberNameOverride );
                }

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentTypeStack.Peek().Type ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( this._currentInsertPosition!.Value.SyntaxNode != null ? GetNodeId( this._currentInsertPosition.Value.SyntaxNode ) : null );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionRelation ).Returns( this._currentInsertPosition.Value.Relation );

                A.CallTo( () => ((ITestTransformation) transformation).IntroducedElementName ).Returns( introducedElementName );

                var symbolHelperId = GetNodeId( symbolHelperDeclaration );
                this._currentInsertPosition = new InsertPosition( InsertPositionRelation.Within, (MemberDeclarationSyntax) introductionSyntax.Parent! );
                A.CallTo( () => ((ITestTransformation) transformation).SymbolHelperNodeId ).Returns( symbolHelperId );

                if ( replacedAttribute != null )
                {
                    this._replacedTransformations.Add( (IObservableTransformation) transformation );
                }
                else
                {
                    this._observableTransformations.Add( (IObservableTransformation) transformation );
                }

                return symbolHelperDeclaration;
            }

            private static MemberDeclarationSyntax GetFinalIntroductionSyntax( MemberDeclarationSyntax introductionSyntax, string? memberNameOverride )
            {
                if ( memberNameOverride == null )
                {
                    return introductionSyntax;
                }

                switch ( introductionSyntax )
                {
                    case PropertyDeclarationSyntax propertyDecl:
                        return propertyDecl.WithIdentifier( Identifier( memberNameOverride ) );

                    case FieldDeclarationSyntax fieldDecl:
                        return
                            fieldDecl.WithDeclaration(
                                fieldDecl.Declaration.WithVariables(
                                    SingletonSeparatedList( fieldDecl.Declaration.Variables.Single().WithIdentifier( Identifier( memberNameOverride ) ) ) ) );

                    default:
                        throw new AssertionFailedException();
                }
            }

            private MemberDeclarationSyntax ProcessPseudoOverride(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax overrideAttribute,
                bool notInlineable,
                bool notDiscardable )
            {
                if ( overrideAttribute.ArgumentList == null || overrideAttribute.ArgumentList.Arguments.Count < 2
                                                            || overrideAttribute.ArgumentList.Arguments.Count > 3 )
                {
                    throw new ArgumentException(
                        "PseudoOverride should have 2 or 3 arguments - overridden declaration name, aspect name and optionally layer name." );
                }

                var overriddenDeclarationName =
                    ((InvocationExpressionSyntax) overrideAttribute.ArgumentList.Arguments[0].Expression).ArgumentList.Arguments[0].ToString();

                var aspectName = overrideAttribute.ArgumentList.Arguments[1].ToString().Trim( '\"' );

                string? layerName = null;

                if ( overrideAttribute.ArgumentList.Arguments.Count == 3 )
                {
                    layerName = overrideAttribute.ArgumentList.Arguments[2].ToString().Trim( '\"' );
                }

                var aspectLayer = this._owner.GetOrAddAspectLayer( aspectName.AssertNotNull(), layerName );

                var transformation = (IIntroduceMemberTransformation) A.Fake<object>(
                    o => o
                        .Implements<INonObservableTransformation>()
                        .Implements<IIntroduceMemberTransformation>()
                        .Implements<IOverriddenDeclaration>()
                        .Implements<ITestTransformation>() );

                DeclarationKind declarationKind;

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

                        declarationKind = DeclarationKind.Method;

                        break;

                    case MethodDeclarationSyntax { ExpressionBody: not null } method:
                        var rewrittenMethodExpressionBody = methodBodyRewriter.VisitArrowExpressionClause( method.ExpressionBody.AssertNotNull() );

                        overrideSyntax =
                            method
                                .WithAttributeLists( List( newAttributeLists ) )
                                .WithExpressionBody( (ArrowExpressionClauseSyntax) rewrittenMethodExpressionBody.AssertNotNull() );

                        declarationKind = DeclarationKind.Method;

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

                        declarationKind = DeclarationKind.Property;

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

                        declarationKind = DeclarationKind.Event;

                        break;

                    case EventFieldDeclarationSyntax eventField:
                        overrideSyntax =
                            eventField
                                .WithAttributeLists( List( newAttributeLists ) );

                        declarationKind = DeclarationKind.Event;

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
                                declarationKind,
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

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentTypeStack.Peek().Type ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( this._currentInsertPosition!.Value.SyntaxNode != null ? GetNodeId( this._currentInsertPosition.Value.SyntaxNode ) : null );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionRelation ).Returns( this._currentInsertPosition.Value.Relation );

                A.CallTo( () => ((ITestTransformation) transformation).OverriddenDeclarationName ).Returns( overriddenDeclarationName );
                A.CallTo( () => ((ITestTransformation) transformation).SymbolHelperNodeId ).Returns( GetNodeId( symbolHelperDeclaration ) );

                this._nonObservableTransformations.Add( (INonObservableTransformation) transformation );

                return symbolHelperDeclaration;
            }

            private static MemberDeclarationSyntax GetSymbolHelperDeclaration( MemberDeclarationSyntax node, string? memberNameOverride = null )
            {
                return (MemberDeclarationSyntax) AssignNodeId(
                    MarkTemporary(
                        node switch
                        {
                            FieldDeclarationSyntax field => GetSymbolHelperField( field, memberNameOverride ),
                            MethodDeclarationSyntax method => GetSymbolHelperMethod( method, memberNameOverride ),
                            PropertyDeclarationSyntax property => GetSymbolHelperProperty( property, memberNameOverride ),
                            EventDeclarationSyntax @event => GetSymbolHelperEvent( @event, memberNameOverride ),
                            EventFieldDeclarationSyntax eventField => GetSymbolHelperEventField( eventField, memberNameOverride ),
                            _ => throw new NotSupportedException()
                        } ) );
            }

            private static SyntaxNode GetSymbolHelperField( FieldDeclarationSyntax field, string? memberNameOverride )
            {
                return field
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithDeclaration(
                        field.Declaration.WithVariables(
                            SeparatedList(
                                field.Declaration.Variables.Select(
                                    v => v.WithIdentifier( Identifier( GetSymbolHelperName( memberNameOverride ?? v.Identifier.ValueText ) ) ) ) ) ) );
            }

            private static SyntaxNode GetSymbolHelperMethod( MethodDeclarationSyntax method, string? memberNameOverride )
            {
                return method
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithModifiers( TokenList( method.Modifiers.Where( m => !m.IsKind( SyntaxKind.OverrideKeyword ) ) ) )
                    .WithIdentifier( Identifier( GetSymbolHelperName( memberNameOverride ?? method.Identifier.ValueText ) ) )
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

            private static SyntaxNode GetSymbolHelperProperty( PropertyDeclarationSyntax property, string? memberNameOverride )
            {
                if ( property.AccessorList != null )
                {
                    return property
                        .WithAttributeLists( List<AttributeListSyntax>() )
                        .WithIdentifier( Identifier( GetSymbolHelperName( memberNameOverride ?? property.Identifier.ValueText ) ) )
                        .WithModifiers( TokenList( property.Modifiers.Where( m => !m.IsKind( SyntaxKind.OverrideKeyword ) ) ) )
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
                        .WithIdentifier( Identifier( GetSymbolHelperName( memberNameOverride ?? property.Identifier.ValueText ) ) )
                        .WithModifiers( TokenList( property.Modifiers.Where( m => !m.IsKind( SyntaxKind.OverrideKeyword ) ) ) )
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                LiteralExpression(
                                    SyntaxKind.DefaultLiteralExpression,
                                    Token( SyntaxKind.DefaultKeyword ) ) ) );
                }
            }

            private static SyntaxNode GetSymbolHelperEvent( EventDeclarationSyntax @event, string? memberNameOverride )
            {
                return @event
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithIdentifier( Identifier( GetSymbolHelperName( memberNameOverride ?? @event.Identifier.ValueText ) ) )
                    .WithModifiers( TokenList( @event.Modifiers.Where( m => !m.IsKind( SyntaxKind.OverrideKeyword ) ) ) )
                    .WithAccessorList( AccessorList( List( @event.AccessorList.AssertNotNull().Accessors.Select( a => a.WithBody( Block() ) ) ) ) );
            }

            private static SyntaxNode GetSymbolHelperEventField( EventFieldDeclarationSyntax eventField, string? memberNameOverride )
            {
                return eventField
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithDeclaration(
                        eventField.Declaration.WithVariables(
                            SeparatedList(
                                eventField.Declaration.Variables.Select(
                                    v => v.WithIdentifier( Identifier( GetSymbolHelperName( memberNameOverride ?? v.Identifier.ValueText ) ) ) ) ) ) );
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
                A.CallTo( () => fakeAspectSymbol.GetAttributes() ).Returns( ImmutableArray<AttributeData>.Empty );
                A.CallTo( () => fakeGlobalNamespaceSymbol.IsGlobalNamespace ).Returns( true );
                A.CallTo( () => fakeGlobalNamespaceSymbol.Kind ).Returns( SymbolKind.Namespace );

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
                        new AspectDriverFactory( fakeCompilation, ImmutableArray<object>.Empty, this._owner.ServiceProvider ) );

                var fakeAspectInstance = new AspectInstance( A.Fake<IAspect>(), default, aspectClass, default );

                return A.Fake<Advice>(
                    i => i.WithArgumentsForConstructor(
                        new object?[]
                        {
                            fakeAspectInstance,
                            fakeAspectInstance.TemplateInstances.Values.Single(),
                            A.Fake<IDeclarationImpl>(),
                            aspectLayer.LayerName,
                            null
                        } ) );
            }
        }
    }
}