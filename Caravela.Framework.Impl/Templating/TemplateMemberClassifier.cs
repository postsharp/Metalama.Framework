﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Project;
using Caravela.Framework.Sdk;
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
        private readonly ISymbolClassifier _symbolClassifier;

        public TemplateMemberClassifier(
            Compilation compilation,
            SemanticAnnotationMap semanticAnnotationMap )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
            this._symbolClassifier = SymbolClassifier.GetInstance( compilation );

            var reflectionMapper = ReflectionMapper.GetInstance( compilation );
            this._templateContextType = reflectionMapper.GetTypeSymbol( typeof(meta) );
        }

        private bool IsCompileTime( ISymbol? symbol )
            => symbol != null && this._symbolClassifier.GetSymbolDeclarationScope( symbol ).DynamicToCompileTimeOnly()
                == SymbolDeclarationScope.CompileTimeOnly;

#pragma warning disable CA1822 // Static anyway.
        public bool IsDynamicType( ITypeSymbol? type ) => type is IDynamicTypeSymbol or IArrayTypeSymbol { ElementType: IDynamicTypeSymbol };
#pragma warning restore CA1822

        public bool IsDynamicParameter( ArgumentSyntax argument )
            => this._semanticAnnotationMap.GetParameterSymbol( argument )?.Type is
                IDynamicTypeSymbol
                or IArrayTypeSymbol { ElementType: IDynamicTypeSymbol };

        public bool IsRunTimeMethod( ISymbol symbol )
            => symbol.Name == nameof(meta.RunTime) &&
               symbol.ContainingType.GetDocumentationCommentId() == this._templateContextType.GetDocumentationCommentId();

        public bool IsRunTimeMethod( SyntaxNode node )
            => this._semanticAnnotationMap.GetSymbol( node ) is IMethodSymbol symbol && this.IsRunTimeMethod( symbol );

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        public bool IsDynamicType( SyntaxNode originalNode )
        {
            var expressionType = this._semanticAnnotationMap.GetExpressionType( originalNode );
            var nodeSymbol = this._semanticAnnotationMap.GetSymbol( originalNode );

            if ( !this.IsCompileTime( nodeSymbol ) )
            {
                // This may be a dynamic member, but a purely run-time one, and we are not interested in those.
                return false;
            }

            if ( this.IsDynamicType( expressionType ) ||
                 (nodeSymbol is IMethodSymbol method && this.IsDynamicType( method.ReturnType )) ||
                 (nodeSymbol is IPropertySymbol property && this.IsDynamicType( property.Type )) )
            {
                return true;
            }
            else
            {
                if ( originalNode is InvocationExpressionSyntax invocation
                     && this._semanticAnnotationMap.GetSymbol( invocation.Expression ) is IMethodSymbol invokedMethod )
                {
                    return invokedMethod.GetReturnTypeAttributes().Any( a => a.AttributeClass?.Name == nameof(RunTimeOnlyAttribute) );
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines if a symbol represents a call to <c>meta.Proceed()</c>.
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

#pragma warning disable CA1822 // Static anyway.

        public bool HasTemplateKeywordAttribute( ISymbol symbol )
            => symbol.GetAttributes()
                .Any( a => a.AttributeClass != null && a.AttributeClass.AnyBaseType( t => t.Name == nameof(TemplateKeywordAttribute) ) );
#pragma warning restore CA1822 

    }
}