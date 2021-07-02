// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Advices;
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
                    }

                    return newNode;
                }

                // Non-pseudo nodes become the next insert positions.
                node = AssignNodeId( node );
                this._currentInsertPosition = node;

                return node;
            }

            private static bool HasPseudoAttribute( MemberDeclarationSyntax node )
            {
                return node.AttributeLists.SelectMany( x => x.Attributes ).Any( x => x.Name.ToString().StartsWith( "Pseudo" ) );
            }

            private SyntaxNode? ProcessPseudoAttributeNode( MemberDeclarationSyntax node, out bool isPseudoMember )
            {
                var newAttributeLists = new List<AttributeListSyntax>();
                AttributeSyntax? pseudoIntroductionAttribute = null;
                AttributeSyntax? pseudoOverrideAttribute = null;

                var forceNotInlineable = false;

                foreach ( var attributeList in node.AttributeLists )
                {
                    var newAttributes = new List<AttributeSyntax>();

                    // First pass: option attributes
                    foreach ( var attribute in attributeList.Attributes )
                    {
                        var name = attribute.Name.ToString();

                        if ( name == "PseudoForceNotInlineable" )
                        {
                            forceNotInlineable = true;
                        }
                    }

                    // Second pass: transformation attributes
                    foreach ( var attribute in attributeList.Attributes )
                    {
                        var name = attribute.Name.ToString();

                        if ( name == "PseudoIntroduction" )
                        {
                            pseudoIntroductionAttribute = attribute;
                        }
                        else if ( name == "PseudoOverride" )
                        {
                            pseudoOverrideAttribute = attribute;
                        }
                        else if ( name.StartsWith( "Pseudo" ) && name != "PseudoForceNotInlineable" )
                        {
                            throw new NotSupportedException( $"Unsupported pseudo attribute {name}" );
                        }
                        else if ( name != "PseudoForceNotInlineable" )
                        {
                            newAttributes.Add( attribute );
                        }
                    }

                    if ( newAttributes.Count > 0 )
                    {
                        newAttributeLists.Add( attributeList.WithAttributes( SeparatedList( newAttributes ) ) );
                    }
                }

                if ( pseudoIntroductionAttribute != null )
                {
                    // Introduction will create a temporary declaration, that will help us to provide values for IMethod member.
                    isPseudoMember = true;

                    return this.ProcessPseudoIntroduction( node, newAttributeLists, pseudoIntroductionAttribute, forceNotInlineable );
                }
                else if ( pseudoOverrideAttribute != null )
                {
                    isPseudoMember = true;

                    return this.ProcessPseudoOverride( node, newAttributeLists, pseudoOverrideAttribute, forceNotInlineable );
                }

                isPseudoMember = false;

                var transformedNode = node.WithAttributeLists( List( newAttributeLists ) );

                if ( forceNotInlineable )
                {
                    transformedNode = transformedNode.WithAdditionalAnnotations( LinkerAnalysisRegistry.DoNotInlineAnnotation );
                }

                return transformedNode;
            }

            private MemberDeclarationSyntax ProcessPseudoIntroduction(
                MemberDeclarationSyntax node,
                List<AttributeListSyntax> newAttributeLists,
                AttributeSyntax attribute,
                bool forceNotInlineable )
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
                                AspectLinkerOptions.Create( forceNotInlineable ),
                                null )
                        } );

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentType.AssertNotNull() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( GetNodeId( this._currentInsertPosition.AssertNotNull() ) );

                var introducedElementName = node switch
                {
                    MethodDeclarationSyntax method => method.Identifier.ValueText,
                    PropertyDeclarationSyntax property => property.Identifier.ValueText,
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
                bool forceNotInlineable )
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
                    case MethodDeclarationSyntax method:
                        var rewrittenMethodBody = methodBodyRewriter.VisitBlock( method.Body.AssertNotNull() );

                        overrideSyntax =
                            method
                                .WithAttributeLists( List( newAttributeLists ) )
                                .WithBody( (BlockSyntax) rewrittenMethodBody.AssertNotNull() );

                        break;

                    // TODO: All cases for properties.
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

                    default:
                        throw new NotSupportedException();
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
                                    _ => throw new NotSupportedException()
                                },
                                AspectLinkerOptions.Create( forceNotInlineable ),
                                null )
                        } );

                A.CallTo( () => ((ITestTransformation) transformation).ContainingNodeId ).Returns( GetNodeId( this._currentType.AssertNotNull() ) );

                A.CallTo( () => ((ITestTransformation) transformation).InsertPositionNodeId )
                    .Returns( GetNodeId( this._currentInsertPosition.AssertNotNull() ) );

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
                            MethodDeclarationSyntax method => GetSymbolHelperMethod( method ),
                            PropertyDeclarationSyntax property => GetSymbolHelperProperty( property ),
                            _ => throw new NotSupportedException()
                        } ) );
            }

            private static SyntaxNode GetSymbolHelperMethod( MethodDeclarationSyntax method )
            {
                return method
                    .WithAttributeLists( List<AttributeListSyntax>() )
                    .WithIdentifier( Identifier( method.Identifier.ValueText + "__SymbolHelper" ) )
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
                                                a.WithBody(
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
        }
    }
}