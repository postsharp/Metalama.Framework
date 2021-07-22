// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Caravela.Framework.Tests.UnitTests.Linker.Helpers
{
    public partial class LinkerTestBase
    {
        private class TestTypeRewriter : CSharpSyntaxRewriter
        {
            private readonly List<IObservableTransformation> _observableTransformations;
            private readonly List<INonObservableTransformation> _nonObservableTransformations;

            private readonly TestRewriter _owner;
            private TypeDeclarationSyntax? _currentType;
            private MemberDeclarationSyntax? _currentInsertPosition;
            private InsertPositionRelation _currentInsertPositionRelation;

            public IReadOnlyList<IObservableTransformation> ObservableTransformations => this._observableTransformations;

            public IReadOnlyList<INonObservableTransformation> NonObservableTransformations => this._nonObservableTransformations;

            public TestTypeRewriter( TestRewriter owner )
            {
                this._owner = owner;
                this._observableTransformations = new List<IObservableTransformation>();
                this._nonObservableTransformations = new List<INonObservableTransformation>();
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var rewrittenNode = this.ProcessTypeDeclaration( node );

                return base.VisitClassDeclaration( (ClassDeclarationSyntax) rewrittenNode );
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                var rewrittenNode = this.ProcessTypeDeclaration( node );

                return base.VisitRecordDeclaration( (RecordDeclarationSyntax) rewrittenNode );
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                var rewrittenNode = this.ProcessTypeDeclaration( node );

                return base.VisitStructDeclaration( (StructDeclarationSyntax) rewrittenNode );
            }

            private TypeDeclarationSyntax ProcessTypeDeclaration( TypeDeclarationSyntax node )
            {
                node = AssignNodeId( node );

                this._currentType = node;
                this._currentInsertPosition = node;
                this._currentInsertPositionRelation = InsertPositionRelation.Within;

                return node;
            }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( HasPseudoAttribute( node ) )
                {
                    var newNode = this.ProcessPseudoAttributeNode( node, out var isPseudoMember );

                    if ( !isPseudoMember )
                    {
                        newNode = AssignNodeId( newNode.AssertNotNull() );
                        this._currentInsertPosition = (MemberDeclarationSyntax) newNode.AssertNotNull();
                    }

                    return newNode;
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = node;
                this._currentInsertPositionRelation = InsertPositionRelation.After;

                return node;
            }

            public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                if ( HasPseudoAttribute( node ) )
                {
                    var newNode = this.ProcessPseudoAttributeNode( node, out var isPseudoMember );

                    if ( !isPseudoMember )
                    {
                        newNode = AssignNodeId( newNode.AssertNotNull() );
                        this._currentInsertPosition = (MemberDeclarationSyntax) newNode.AssertNotNull();
                        this._currentInsertPositionRelation = InsertPositionRelation.After;
                    }

                    return newNode;
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = node;
                this._currentInsertPositionRelation = InsertPositionRelation.After;

                return node;
            }

            public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
            {
                if ( HasPseudoAttribute( node ) )
                {
                    var newNode = this.ProcessPseudoAttributeNode( node, out var isPseudoMember );

                    if ( !isPseudoMember )
                    {
                        newNode = AssignNodeId( newNode.AssertNotNull() );
                        this._currentInsertPosition = (MemberDeclarationSyntax) newNode.AssertNotNull();
                        this._currentInsertPositionRelation = InsertPositionRelation.After;
                    }

                    return newNode;
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = node;
                this._currentInsertPositionRelation = InsertPositionRelation.After;

                return node;
            }

            public override SyntaxNode? VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                if ( HasPseudoAttribute( node ) )
                {
                    var newNode = this.ProcessPseudoAttributeNode( node, out var isPseudoMember );

                    if ( !isPseudoMember )
                    {
                        newNode = AssignNodeId( newNode.AssertNotNull() );
                        this._currentInsertPosition = (MemberDeclarationSyntax) newNode.AssertNotNull();
                        this._currentInsertPositionRelation = InsertPositionRelation.After;
                    }

                    return newNode;
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = node;
                this._currentInsertPositionRelation = InsertPositionRelation.After;

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

                foreach ( var attributeList in node.AttributeLists )
                {
                    // First pass: option attributes
                    foreach ( var attribute in attributeList.Attributes )
                    {
                        var name = attribute.Name.ToString();

                        if ( name == "PseudoNotInlineable" )
                        {
                            notInlineable = true;
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
                                  !string.Equals( name, "PseudoNotInlineable", StringComparison.Ordinal ) )
                        {
                            throw new NotSupportedException( $"Unsupported pseudo attribute {name}" );
                        }
                    }
                }

                if ( pseudoIntroductionAttribute != null )
                {
                    // Introduction will create a temporary declaration, that will help us to provide values for IMethod member.
                    isPseudoMember = true;

                    return this.ProcessPseudoIntroduction( node, newAttributeLists, pseudoIntroductionAttribute, notInlineable );
                }
                else if ( pseudoOverrideAttribute != null )
                {
                    isPseudoMember = true;

                    return this.ProcessPseudoOverride( node, newAttributeLists, pseudoOverrideAttribute, notInlineable );
                }
                else
                {
                    isPseudoMember = false;

                    var transformedNode = node.WithAttributeLists( List( newAttributeLists ) );

                    if ( notInlineable )
                    {
                        transformedNode = transformedNode.WithLinkerDeclarationFlags( LinkerDeclarationFlags.NotInlineable );
                    }

                    return transformedNode;
                }
            }

            private MemberDeclarationSyntax ProcessPseudoIntroduction(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax attribute,
                bool notInlineable )
            {
                if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count < 1 || attribute.ArgumentList.Arguments.Count > 3 )
                {
                    throw new ArgumentException( "PseudoIntroduction should have 1 or 2 arguments - aspect name and optionally layer name." );
                }

                var aspectName = attribute.ArgumentList.Arguments[0].ToString();

                string? layerName = null;

                if ( attribute.ArgumentList.Arguments.Count == 2 )
                {
                    layerName = attribute.ArgumentList.Arguments[1].ToString();
                }

                this._owner.AddAspectLayer( aspectName.AssertNotNull(), layerName );

                var symbolHelperDeclaration = GetSymbolHelperDeclaration( node );

                var introductionSyntax = node.WithAttributeLists( List( newAttributeLists ) );

                if ( notInlineable )
                {
                    introductionSyntax = introductionSyntax.WithLinkerDeclarationFlags( LinkerDeclarationFlags.NotInlineable );
                }

                // Create transformation fake.
                var transformation = (IMemberIntroduction) A.Fake<object>(
                    o => o
                        .Implements<IObservableTransformation>()
                        .Implements<IMemberIntroduction>()
                        .Implements<IMethod>()
                        .Implements<IDeclarationInternal>()
                        .Implements<ITestTransformation>() );

                A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
                A.CallTo( () => transformation.ToString() ).Returns( "Introduced" );
                A.CallTo( () => transformation.TargetSyntaxTree ).Returns( node.SyntaxTree );

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

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentType.AssertNotNull() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( GetNodeId( this._currentInsertPosition.AssertNotNull() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionRelation ).Returns( this._currentInsertPositionRelation );

                var introducedElementName = node switch
                {
                    MethodDeclarationSyntax method => method.Identifier.ValueText,
                    PropertyDeclarationSyntax property => property.Identifier.ValueText,
                    EventDeclarationSyntax @event => @event.Identifier.ValueText,
                    EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables.Single().Identifier.ValueText,
                    _ => throw new NotSupportedException()
                };

                A.CallTo( () => ((ITestTransformation) transformation).IntroducedElementName ).Returns( introducedElementName );
                A.CallTo( () => ((ITestTransformation) transformation).SymbolHelperNodeId ).Returns( GetNodeId( symbolHelperDeclaration ) );

                this._observableTransformations.Add( (IObservableTransformation) transformation );

                return symbolHelperDeclaration;
            }

            private MemberDeclarationSyntax ProcessPseudoOverride(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax attribute,
                bool notInlineable )
            {
                if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count < 2 || attribute.ArgumentList.Arguments.Count > 3 )
                {
                    throw new ArgumentException(
                        "PseudoOverride should have 2 or 3 arguments - overridden declaration name, aspect name and optionally layer name." );
                }

                var overriddenDeclarationName = attribute.ArgumentList.Arguments[0].ToString();
                var aspectName = attribute.ArgumentList.Arguments[1].ToString();

                string? layerName = null;

                if ( attribute.ArgumentList.Arguments.Count == 3 )
                {
                    layerName = attribute.ArgumentList.Arguments[2].ToString();
                }

                this._owner.AddAspectLayer( aspectName.AssertNotNull(), layerName );

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

                if ( notInlineable )
                {
                    overrideSyntax = overrideSyntax.WithLinkerDeclarationFlags( LinkerDeclarationFlags.NotInlineable );
                }

                var symbolHelperDeclaration = GetSymbolHelperDeclaration( node );

                A.CallTo( () => transformation.GetHashCode() ).Returns( 0 );
                A.CallTo( () => transformation.ToString() ).Returns( "Override" );

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

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentType.AssertNotNull() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( GetNodeId( this._currentInsertPosition.AssertNotNull() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionRelation ).Returns( this._currentInsertPositionRelation );

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
        }
    }
}