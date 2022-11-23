// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating
{
    /// <summary>
    /// Provides methods that tests for classifications of template members, for instance <see cref="IsRunTimeMethod"/>.
    /// </summary>
    internal class TemplateMemberClassifier : TemplateMemberSymbolClassifier
    {
        private readonly SyntaxTreeAnnotationMap _syntaxTreeAnnotationMap;

        public TemplateMemberClassifier(
            Compilation runTimeCompilation,
            SyntaxTreeAnnotationMap syntaxTreeAnnotationMap,
            ProjectServiceProvider serviceProvider ) : base( runTimeCompilation, serviceProvider )
        {
            this._syntaxTreeAnnotationMap = syntaxTreeAnnotationMap;
        }

        public bool IsDynamicParameter( ArgumentSyntax argument ) => IsDynamicParameter( this._syntaxTreeAnnotationMap.GetParameterSymbol( argument )?.Type );

        public bool IsTemplateParameter( ExpressionSyntax expression )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( expression );

            if ( symbol is IParameterSymbol parameter )
            {
                return IsTemplateParameter( parameter );
            }
            else
            {
                return false;
            }
        }

        public bool IsRunTimeMethod( SyntaxNode node )
            => this._syntaxTreeAnnotationMap.GetSymbol( node ) is IMethodSymbol symbol && this.IsRunTimeMethod( symbol );

        public bool IsNodeOfDynamicType( SyntaxNode originalNode ) => originalNode is ExpressionSyntax expression && this.IsNodeOfDynamicType( expression );

        /// <summary>
        /// Determines if a node is of <c>dynamic</c> type.
        /// </summary>
        /// <param name="originalNode"></param>
        /// <returns></returns>
        public bool IsNodeOfDynamicType( ExpressionSyntax originalNode )
        {
            var expressionType = this._syntaxTreeAnnotationMap.GetExpressionType( originalNode );

            if ( expressionType is IDynamicTypeSymbol )
            {
                // Roslyn returns a dynamic type even for methods returning a non-dynamic type, as long as they have at least
                // one dynamic argument. We don't want to fix the Roslyn type resolution, but in the specific case of void methods,
                // we can do it without a chance of being ever wrong. It allows meta.DefineExpression to work.
                if ( originalNode is InvocationExpressionSyntax &&
                     this._syntaxTreeAnnotationMap.GetSymbol( originalNode ) is IMethodSymbol { ReturnsVoid: true } )
                {
                    return false;
                }
            }

            if ( expressionType != null && this.SymbolClassifier.GetTemplatingScope( expressionType ) == TemplatingScope.Dynamic )
            {
                return true;
            }

            var nodeSymbol = this._syntaxTreeAnnotationMap.GetSymbol( originalNode );

            return (nodeSymbol is IMethodSymbol method && this.SymbolClassifier.GetTemplatingScope( method.ReturnType ) == TemplatingScope.Dynamic) ||
                   (nodeSymbol is IPropertySymbol property && this.SymbolClassifier.GetTemplatingScope( property.Type ) == TemplatingScope.Dynamic);
        }

        /// <summary>
        /// Determines if the node is a pragma and returns the kind of pragma, if any.
        /// </summary>
        public MetaMemberKind GetMetaMemberKind( SyntaxNode node )
        {
            var symbol = this._syntaxTreeAnnotationMap.GetSymbol( node );

            return this.GetMetaMemberKind( symbol );
        }

        private MetaMemberKind GetMetaMemberKind( ISymbol? symbol )
        {
            if ( symbol == null )
            {
                return MetaMemberKind.None;
            }
            else if ( SymbolEqualityComparer.Default.Equals( symbol.ContainingType, this.MetaType ) )
            {
                switch ( symbol.Name )
                {
                    case nameof(meta.This):
                        return MetaMemberKind.This;

                    case nameof(meta.InsertComment):
                        return MetaMemberKind.InsertComment;

                    case nameof(meta.InsertStatement):
                        return MetaMemberKind.InsertStatement;

                    case nameof(meta.Proceed):
                        return MetaMemberKind.Proceed;

                    case nameof(meta.ProceedAsync):
                        return MetaMemberKind.ProceedAsync;

                    case nameof(meta.ProceedEnumerable):
                        return MetaMemberKind.ProceedEnumerable;

                    case nameof(meta.ProceedEnumerator):
                        return MetaMemberKind.ProceedEnumerator;

                    case "ProceedAsyncEnumerable":
                        return MetaMemberKind.ProceedAsyncEnumerable;

                    case "ProceedAsyncEnumerator":
                        return MetaMemberKind.ProceedAsyncEnumerator;

                    default:
                        return MetaMemberKind.Default;
                }
            }
            else
            {
                return MetaMemberKind.None;
            }
        }
    }
}