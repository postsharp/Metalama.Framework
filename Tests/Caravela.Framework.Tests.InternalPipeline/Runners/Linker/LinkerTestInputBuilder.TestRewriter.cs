// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Tests.Integration.Tests.Linker;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable CA1305 // Specify IFormatProvider

namespace Caravela.Framework.Tests.Integration.Runners.Linker
{
    internal partial class LinkerTestInputBuilder
    {
        private const string _testNodeIdAnnotationId = "LinkerTestRewriterNodeId";
        private const string _testTemporaryNodeAnnotationId = "LinkerTestTemporaryNode";
        private static int _nextNodeId;

        public static T AssignNodeId<T>( T node )
            where T : SyntaxNode
        {
            if ( node.GetAnnotations( _testNodeIdAnnotationId ).Any() )
            {
                return node;
            }

            var id = Interlocked.Increment( ref _nextNodeId ).ToString();

            return node.WithAdditionalAnnotations( new SyntaxAnnotation( _testNodeIdAnnotationId, id ) );
        }

        private static string GetNodeId( SyntaxNode node )
        {
            return node.GetAnnotations( _testNodeIdAnnotationId ).Select( x => x.Data.AssertNotNull() ).Single();
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

        /// <summary>
        /// Rewrites method bodies, replacing call to pseudo method called "annotate" with linker annotation.
        /// </summary>
        private class TestRewriter : CSharpSyntaxRewriter
        {
            private readonly List<AspectLayerId> _orderedAspectLayers;
            private readonly List<IObservableTransformation> _observableTransformations;
            private readonly List<INonObservableTransformation> _nonObservableTransformations;

            public IReadOnlyList<IObservableTransformation> ObservableTransformations => this._observableTransformations;

            public IReadOnlyList<INonObservableTransformation> NonObservableTransformations => this._nonObservableTransformations;

            public IReadOnlyList<AspectLayerId> OrderedAspectLayers => this._orderedAspectLayers;

            public ServiceProvider ServiceProvider { get; }

            public TestRewriter()
            {
                this._orderedAspectLayers = new List<AspectLayerId>();
                this._observableTransformations = new List<IObservableTransformation>();
                this._nonObservableTransformations = new List<INonObservableTransformation>();

                this.ServiceProvider = new ServiceProvider();

                this.ServiceProvider.AddService( new Impl.Utilities.UserCodeInvoker( this.ServiceProvider ) );
            }

            public override SyntaxNode? VisitUsingDirective( UsingDirectiveSyntax node )
            {
                if ( !node.StaticKeyword.IsMissing && StringComparer.Ordinal.Equals( node.Name.ToFullString(), typeof(Api).FullName ) )
                {
                    return null;
                }

                return base.VisitUsingDirective( node );
            }

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                if ( HasLayerOrderAttribute( node ) )
                {
                    node = (ClassDeclarationSyntax) this.ProcessLayerOrderAttributeNode( node ).AssertNotNull();
                }

                var typeRewriter = new TestTypeRewriter( this );

                var ret = typeRewriter.VisitClassDeclaration( node );

                this._observableTransformations.AddRange( typeRewriter.ObservableTransformations );
                this._nonObservableTransformations.AddRange( typeRewriter.NonObservableTransformations );

                return ret;
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                if ( HasLayerOrderAttribute( node ) )
                {
                    node = (RecordDeclarationSyntax) this.ProcessLayerOrderAttributeNode( node ).AssertNotNull();
                }

                var typeRewriter = new TestTypeRewriter( this );

                var ret = typeRewriter.VisitRecordDeclaration( node );

                this._observableTransformations.AddRange( typeRewriter.ObservableTransformations );
                this._nonObservableTransformations.AddRange( typeRewriter.NonObservableTransformations );

                return ret;
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                if ( HasLayerOrderAttribute( node ) )
                {
                    node = (StructDeclarationSyntax) this.ProcessLayerOrderAttributeNode( node ).AssertNotNull();
                }

                var typeRewriter = new TestTypeRewriter( this );

                var ret = typeRewriter.VisitStructDeclaration( node );

                this._observableTransformations.AddRange( typeRewriter.ObservableTransformations );
                this._nonObservableTransformations.AddRange( typeRewriter.NonObservableTransformations );

                return ret;
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
                            if ( attribute.ArgumentList == null || attribute.ArgumentList.Arguments.Count == 0 || attribute.ArgumentList.Arguments.Count > 3 )
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