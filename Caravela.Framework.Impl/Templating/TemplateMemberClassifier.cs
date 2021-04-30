// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Provides methods that tests for classifications of template members, for instance <see cref="IsRunTimeMethod(ISymbol)"/> or <see cref="IsProceed(SyntaxNode)"/>.
    /// </summary>
    internal class TemplateMemberClassifier
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;
        private readonly ITypeSymbol _templateContextType;

        public TemplateMemberClassifier(
            Compilation compilation,
            SemanticAnnotationMap semanticAnnotationMap )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;

            var reflectionMapper = ReflectionMapper.GetInstance( compilation );
            this._templateContextType = reflectionMapper.GetTypeSymbol( typeof(TemplateContext) );
        }

        public bool IsRunTimeMethod( ISymbol symbol )
            => symbol.Name == nameof(TemplateContext.runTime) &&

               // TODO: symbol comparison does not work here.
               symbol.ContainingType.GetDocumentationCommentId() == this._templateContextType.GetDocumentationCommentId();

        public bool IsRunTimeMethod( SyntaxNode node )
            => this._semanticAnnotationMap.GetSymbol( node ) is IMethodSymbol symbol && this.IsRunTimeMethod( symbol );

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        public bool ReturnsRunTimeOnlyValue( SyntaxNode originalNode )
        {
            if ( this._semanticAnnotationMap.GetExpressionType( originalNode ) is IDynamicTypeSymbol )
            {
                return true;
            }
            else
            {
                if ( originalNode is InvocationExpressionSyntax invocation
                     && this._semanticAnnotationMap.GetSymbol( invocation.Expression ) is IMethodSymbol method )
                {
                    return method.GetReturnTypeAttributes().Any( a => a.AttributeClass?.Name == nameof(RunTimeOnlyAttribute) );
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines if a symbol represents a call to <c>proceed()</c>.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsProceed( SyntaxNode node )
        {
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            if ( symbol == null )
            {
                return false;
            }

            return symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(ProceedAttribute) );
        }
    }
}