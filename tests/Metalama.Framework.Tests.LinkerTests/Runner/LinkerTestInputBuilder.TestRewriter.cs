﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Tests.LinkerTests.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable CA1305 // Specify IFormatProvider

namespace Metalama.Framework.Tests.LinkerTests.Runner
{
    internal partial class LinkerTestInputBuilder
    {
        private const string _testNodeIdAnnotationId = "LinkerTestRewriterNodeId";
        private const string _testTemporaryNodeAnnotationId = "LinkerTestTemporaryNode";
        private static int _nextNodeId;

        private static string AllocateNodeId()
        {
            return Interlocked.Increment( ref _nextNodeId ).ToString();
        }

        private static T AssignNodeId<T>( T node, string nodeId )
            where T : SyntaxNode
        {
            if ( node is EventFieldDeclarationSyntax eventFieldDecl )
            {
                var declarator = eventFieldDecl.Declaration.Variables.Single();
                declarator = AssignNodeId( declarator, nodeId );

                return (T) (SyntaxNode) eventFieldDecl
                    .WithDeclaration( eventFieldDecl.Declaration.WithVariables( SeparatedList( new[] { declarator } ) ) );
            }
            else
            {
                if ( node.GetAnnotations( _testNodeIdAnnotationId ).Any() )
                {
                    return node;
                }

                return node.WithAdditionalAnnotations( new SyntaxAnnotation( _testNodeIdAnnotationId, nodeId ) );
            }
        }

        private static string GetNodeId( SyntaxNode node )
        {
            if ( node is EventFieldDeclarationSyntax eventFieldDecl )
            {
                var declarator = eventFieldDecl.Declaration.Variables.Single();

                return GetNodeId( declarator );
            }
            else
            {
                return node.GetAnnotations( _testNodeIdAnnotationId ).Select( x => x.Data.AssertNotNull() ).Single();
            }
        }

        private static IEnumerable<SyntaxNode> GetNodesWithId( SyntaxTree tree )
        {
            return tree.GetRoot().GetAnnotatedNodes( _testNodeIdAnnotationId );
        }

        private static SyntaxNode MarkTemporary( SyntaxNode node )
        {
            return node.WithAdditionalAnnotations( new SyntaxAnnotation( _testTemporaryNodeAnnotationId, "temporary" ) );
        }

        private static bool IsTemporary( SyntaxNode node )
        {
            return node.GetAnnotations( _testTemporaryNodeAnnotationId ).Any();
        }

        private static string GetSymbolHelperName( string name )
        {
            return name + "__SymbolHelper";
        }

        private static string GetReplacedMemberName( string name )
        {
            return name + "__Replaced";
        }

        /// <summary>
        /// Catalogues all transformations defined by [Pseudo] attributes and removes them from syntax trees. 
        /// Also marks non-pseudo nodes with IDs, which are later used to create insert positions.
        /// </summary>
        private sealed class TestRewriter : SafeSyntaxRewriter
        {
            private readonly List<AspectLayerId> _orderedAspectLayers;

            public IReadOnlyList<AspectLayerId> OrderedAspectLayers => this._orderedAspectLayers;

            public ProjectServiceProvider ServiceProvider { get; }

            public CompilationContext InputCompilation { get; }

            public LinkerTestInputBuilder Builder { get; }

            public TestRewriter( in ProjectServiceProvider serviceProvider, LinkerTestInputBuilder builder, CompilationContext inputCompilation )
            {
                this.Builder = builder;
                this.InputCompilation = inputCompilation;

                this._orderedAspectLayers = new List<AspectLayerId>();

                this.ServiceProvider = serviceProvider;
            }

            public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node )
            {
                // This removes the static using of linker helpers.
                if ( !node.StaticKeyword.IsMissing && StringComparer.Ordinal.Equals( node.Name?.ToString(), typeof(Api).FullName ) )
                {
                    return null;
                }

                return base.VisitUsingDirective( node );
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                var typeRewriter = new TestTypeRewriter( this );

                var newNode = (ClassDeclarationSyntax) typeRewriter.VisitClassDeclaration( node );

                if ( HasLayerOrderAttribute( node ) )
                {
                    newNode = (ClassDeclarationSyntax) this.ProcessLayerOrderAttributeNode( newNode ).AssertNotNull();
                }

                return newNode;
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {

                var typeRewriter = new TestTypeRewriter( this );

                var newNode = (RecordDeclarationSyntax) typeRewriter.VisitRecordDeclaration( node );

                if ( HasLayerOrderAttribute( node ) )
                {
                    newNode = (RecordDeclarationSyntax) this.ProcessLayerOrderAttributeNode( newNode ).AssertNotNull();
                }

                return newNode;
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {

                var typeRewriter = new TestTypeRewriter( this );

                var newNode = (StructDeclarationSyntax) typeRewriter.VisitStructDeclaration( node );

                if ( HasLayerOrderAttribute( node ) )
                {
                    newNode = (StructDeclarationSyntax) this.ProcessLayerOrderAttributeNode( newNode ).AssertNotNull();
                }

                return newNode;
            }

            private static bool HasLayerOrderAttribute( TypeDeclarationSyntax node )
            {
                return node.AttributeLists.SelectMany( x => x.Attributes ).Any( x => x.Name.ToString() == "PseudoLayerOrder" );
            }

            public AspectLayerId GetOrAddAspectLayer( string aspectName, string? layerName )
            {
                if ( !this._orderedAspectLayers.Any( x => x.AspectName == aspectName && x.LayerName == layerName ) )
                {
                    var newLayer = new AspectLayerId( aspectName, layerName );
                    this._orderedAspectLayers.Add( newLayer );

                    return newLayer;
                }
                else
                {
                    return this._orderedAspectLayers.Single( x => x.AspectName == aspectName && x.LayerName == layerName );
                }
            }

            private TypeDeclarationSyntax ProcessLayerOrderAttributeNode( TypeDeclarationSyntax node )
            {
                var newAttributeLists = new List<AttributeListSyntax>();

                foreach ( var attributeList in node.AttributeLists )
                {
                    var newAttributes = new List<AttributeSyntax>();

                    foreach ( var attribute in attributeList.Attributes )
                    {
                        if ( attribute.Name.ToString() == "PseudoLayerOrder" )
                        {
                            if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count is 0 or > 3 )
                            {
                                throw new ArgumentException( "Incorrect number of arguments on LayerOrder" );
                            }

                            var aspectName = attribute.ArgumentList.Arguments[0].ToString().Trim( '\"' );
                            string? layerName = null;

                            if ( attribute.ArgumentList.Arguments.Count == 2 )
                            {
                                layerName = attribute.ArgumentList.Arguments[1].ToString().Trim( '\"' );
                            }

                            this.GetOrAddAspectLayer( aspectName, layerName );
                        }
                        else
                        {
                            newAttributes.Add( attribute );
                        }
                    }

                    if ( attributeList.Attributes.SequenceEqual( newAttributes ) )
                    {
                        newAttributeLists.Add( attributeList );
                    }
                    else
                    {
                        if ( newAttributes.Count > 0 )
                        {
                            newAttributeLists.Add( attributeList.WithAttributes( SeparatedList( newAttributes ) ) );
                        }
                    }
                }

                return node.WithAttributeLists( List( newAttributeLists ) );
            }
        }
    }
}