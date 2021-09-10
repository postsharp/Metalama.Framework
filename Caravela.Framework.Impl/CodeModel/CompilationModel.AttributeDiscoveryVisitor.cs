// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class CompilationModel
    {
        /// <summary>
        /// Discovers custom attributes in a syntax tree and index them by attribute name.
        /// </summary>
        private class AttributeDiscoveryVisitor : CSharpSyntaxWalker
        {
            private readonly ImmutableMultiValueDictionary<string, AttributeRef>.Builder _builder =
                ImmutableMultiValueDictionary<string, AttributeRef>.CreateBuilder( StringComparer.Ordinal );

            public override void VisitAttribute( AttributeSyntax node )
            {
                var name = node.Name switch
                {
                    SimpleNameSyntax simpleName => simpleName.Identifier.Text,
                    QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.Text,
                    _ => throw new AssertionFailedException()
                };

                name = name.TrimEnd( "Attribute" );
                this._builder.Add( name, new AttributeRef( node ) );

                base.VisitAttribute( node );
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node is ExpressionSyntax or ExpressionSyntax )
                {
                    // Don't visit any expression or statement deeply. 
                }
                else
                {
                    base.Visit( node );
                }
            }

            public ImmutableMultiValueDictionary<string, AttributeRef> GetDiscoveredAttributes() => this._builder.ToImmutable();
        }
    }
}