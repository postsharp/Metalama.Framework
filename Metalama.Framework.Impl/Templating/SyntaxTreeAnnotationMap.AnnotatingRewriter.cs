// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class SyntaxTreeAnnotationMap
    {
        /// <summary>
        /// A <see cref="CSharpSyntaxRewriter"/> that adds annotations.
        /// </summary>
        private class AnnotatingRewriter : CSharpSyntaxRewriter
        {
            private readonly SemanticModel? _semanticModel;
            private readonly SyntaxTreeAnnotationMap _map;
            private readonly bool _isTemplate;

            public AnnotatingRewriter( SemanticModel? semanticModel, SyntaxTreeAnnotationMap map, bool isTemplate )
            {
                this._semanticModel = semanticModel;
                this._map = map;
                this._isTemplate = isTemplate;
            }

            public override SyntaxToken VisitToken( SyntaxToken token )
            {
                return this._map.AddLocationAnnotation( token );
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                return this._map.GetAnnotatedNode( node, base.Visit( node ), this._semanticModel, this._isTemplate );
            }
        }
    }
}