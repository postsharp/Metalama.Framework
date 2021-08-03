// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Provides methods that tests for classifications of template members, for instance <see cref="IsRunTimeMethod(ISymbol)"/>.
    /// </summary>
    internal class TemplateMemberClassifier
    {
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;
        private readonly ITypeSymbol _metaType;
        private readonly ISymbolClassifier _symbolClassifier;

        public TemplateMemberClassifier(
            Compilation runTimeCompilation,
            SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
            IServiceProvider serviceProvider )
        {
            this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
            this._symbolClassifier = serviceProvider.GetService<SymbolClassificationService>().GetClassifier( runTimeCompilation );

            var reflectionMapper = ReflectionMapper.GetInstance( runTimeCompilation );
            this._metaType = reflectionMapper.GetTypeSymbol( typeof(meta) );
        }

        public bool RequiresCompileTimeExecution( ISymbol? symbol )
            => symbol != null && this._symbolClassifier.GetTemplatingScope( symbol ).GetExpressionExecutionScope()
                == TemplatingScope.CompileTimeOnly;

        public bool IsDynamicParameter( ArgumentSyntax argument ) => this._syntaxTreeAnnotationMap.GetParameterSymbol( argument )?.Type.IsDynamic() ?? false;

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

            if ( expressionType.IsDynamic() )
            {
                return true;
            }

            var nodeSymbol = this._syntaxTreeAnnotationMap.GetSymbol( originalNode );

            return (nodeSymbol is IMethodSymbol method && method.ReturnType.IsDynamic()) ||
                   (nodeSymbol is IPropertySymbol property && property.Type.IsDynamic());
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

                    case nameof(meta.Proceed):
                        return MetaMemberKind.Proceed;

                    default:
                        return MetaMemberKind.Default;
                }
            }
        }
    }
}