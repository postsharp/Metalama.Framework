// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Provides methods that tests for classifications of template members, for instance <see cref="IsRunTimeMethod(ISymbol)"/> or <see cref="IsProceed(SyntaxNode)"/>.
    /// </summary>
    internal class TemplateMemberClassifier
    {
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
        private readonly ITypeSymbol _metaType;
        private readonly ISymbolClassifier _symbolClassifier;

        public TemplateMemberClassifier(
            Compilation compilation,
            SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
            IServiceProvider serviceProvider )
        {
            this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
            this._symbolClassifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( compilation );

            var reflectionMapper = ReflectionMapper.GetInstance( compilation );
            this._metaType = reflectionMapper.GetTypeSymbol( typeof(meta) );
        }

        private bool IsCompileTime( ISymbol? symbol )
            => symbol != null && this._symbolClassifier.GetSymbolDeclarationScope( symbol ).DynamicToCompileTimeOnly()
                == SymbolDeclarationScope.CompileTimeOnly;

#pragma warning disable CA1822 // Static anyway.
        public bool IsDynamicType( ITypeSymbol? type ) => type is IDynamicTypeSymbol or IArrayTypeSymbol { ElementType: IDynamicTypeSymbol };
#pragma warning restore CA1822

        public bool IsDynamicParameter( ArgumentSyntax argument )
            => this._syntaxTreeAnnotationMap.GetParameterSymbol( argument )?.Type is
                IDynamicTypeSymbol
                or IArrayTypeSymbol { ElementType: IDynamicTypeSymbol };

        public bool IsRunTimeMethod( ISymbol symbol )
            => symbol.Name == nameof(meta.RunTime) &&
               symbol.ContainingType.GetDocumentationCommentId() == this._metaType.GetDocumentationCommentId();

        public bool IsRunTimeMethod( SyntaxNode node )
            => this._syntaxTreeAnnotationMap.GetSymbol( node ) is IMethodSymbol symbol && this.IsRunTimeMethod( symbol );

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        public bool IsDynamicType( SyntaxNode originalNode )
        {
            var expressionType = this._syntaxTreeAnnotationMap.GetExpressionType( originalNode );
            var nodeSymbol = this._syntaxTreeAnnotationMap.GetSymbol( originalNode );

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
                     && this._syntaxTreeAnnotationMap.GetSymbol( invocation.Expression ) is IMethodSymbol invokedMethod )
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
            // TODO: This class and usages must be removed.

            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

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

        /// <summary>
        /// Determines if the node is a pragma and returns the kind of pragma, if any.
        /// </summary>
        public MetaMemberKind GetMetaMemberKind( SyntaxNode node )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            return this.GetMetaMemberKind( symbol );
        }

        public MetaMemberKind GetMetaMemberKind( ISymbol? symbol )
        {
            if ( symbol == null || !SymbolEqualityComparer.Default.Equals( symbol.ContainingType, this._metaType ) )
            {
                return MetaMemberKind.None;
            }
            else
            {
                switch ( symbol.Name )
                {
                    case nameof(meta.Comment):
                        return MetaMemberKind.Comment;

                    case nameof(meta.This):
                        return MetaMemberKind.This;

                    default:
                        return MetaMemberKind.Default;
                }
            }
        }

        internal bool IsImplicitValueParameter( IdentifierNameSyntax node )
        {
            if ( node.Identifier.Text == "value" )
            {
                var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

                return
                    symbol is IParameterSymbol parameter && parameter.ContainingSymbol is IMethodSymbol method
                                                         && (method.MethodKind == MethodKind.PropertySet || method.MethodKind == MethodKind.EventAdd
                                                                                                         || method.MethodKind == MethodKind.EventRemove);
            }
            else
            {
                return false;
            }
        }
    }
}