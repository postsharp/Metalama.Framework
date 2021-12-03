// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// A forwarding <see cref="CSharpSyntaxRewriter"/> that only forwards
    /// 'interesting' declarations and ignores the other ones. This should be generalized
    /// into something that filters build-time expressions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TemplateRewriterFilter<T> : CSharpSyntaxRewriter
        where T : CSharpSyntaxRewriter
    {
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;

        public T Inner { get; }

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
                .Any( a => a.AttributeClass?.Name == nameof( TemplateAttribute ) || a.AttributeClass?.Name == nameof( InterfaceMemberAttribute ) );

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