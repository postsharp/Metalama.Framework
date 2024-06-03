// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Templating
{
    // TODO: Check why it exists but is not used.
    // ReSharper disable once UnusedType.Global

    /// <summary>
    /// A forwarding <see cref="CSharpSyntaxRewriter"/> that only forwards
    /// 'interesting' declarations and ignores the other ones. This should be generalized
    /// into something that filters build-time expressions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TemplateRewriterFilter<T> : SafeSyntaxRewriter
        where T : CSharpSyntaxRewriter
    {
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;

        private T Inner { get; }

        public TemplateRewriterFilter( SyntaxTreeAnnotationMap syntaxTreeAnnotationMap, T inner )
        {
            this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
            this.Inner = inner;
        }

        private bool IsTemplate( SyntaxNode node )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetDeclaredSymbol( node );

            if ( symbol != null )
            {
                return IsTemplate( symbol );
            }

            return false;
        }

        private static bool IsTemplate( ISymbol symbol )
            => symbol.GetAttributes()
                .Any( a => a.AttributeClass?.Name is nameof(TemplateAttribute) or nameof(InterfaceMemberAttribute) );

        public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            return this.IsTemplate( node ) ? this.Inner.VisitMethodDeclaration( node ) : node;
        }

        public override SyntaxNode? VisitPropertyDeclaration( PropertyDeclarationSyntax node )
        {
            return this.IsTemplate( node ) ? this.Inner.VisitPropertyDeclaration( node ) : node;
        }

        public override SyntaxNode? VisitEventDeclaration( EventDeclarationSyntax node )
        {
            return this.IsTemplate( node ) ? this.Inner.VisitEventDeclaration( node ) : node;
        }
    }
}