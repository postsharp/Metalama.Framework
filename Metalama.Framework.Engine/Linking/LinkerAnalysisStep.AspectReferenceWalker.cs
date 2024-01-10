// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerAnalysisStep
    {
        /// <summary>
        /// Walks method bodies, counting return statements.
        /// </summary>
        private sealed class AspectReferenceWalker : SafeSyntaxWalker
        {
            private readonly AspectReferenceResolver _referenceResolver;
            private readonly ISemanticModel _semanticModel;
            private readonly IMethodSymbol _containingSymbol;
            private readonly Stack<IMethodSymbol> _localFunctionStack;

            public List<ResolvedAspectReference> AspectReferences { get; }

            public AspectReferenceWalker( AspectReferenceResolver referenceResolver, ISemanticModel semanticModel, IMethodSymbol containingSymbol )
            {
                this._referenceResolver = referenceResolver;
                this._semanticModel = semanticModel;
                this.AspectReferences = new List<ResolvedAspectReference>();
                this._containingSymbol = containingSymbol;
                this._localFunctionStack = new Stack<IMethodSymbol>();
            }

            public override void VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
            {
                var symbol = (IMethodSymbol) this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                try
                {
                    this._localFunctionStack.Push( symbol );

                    base.VisitLocalFunctionStatement( node );
                }
                finally
                {
                    this._localFunctionStack.Pop();
                }
            }

            protected override void VisitCore( SyntaxNode? node )
            {
                if ( node == null )
                {
                    // Coverage: ignore (irrelevant).
                    return;
                }

                if ( node.TryGetAspectReference( out var aspectReference ) )
                {
                    var nodeWithSymbol = node switch
                    {
                        ConditionalAccessExpressionSyntax conditionalAccess => GetConditionalMemberName( conditionalAccess ),
                        _ => node
                    };

                    var symbolInfo = this._semanticModel.GetSymbolInfo( nodeWithSymbol );
                    var referencedSymbol = symbolInfo.Symbol;

                    if ( referencedSymbol == null )
                    {
                        // This is a workaround for a problem I don't fully understand.
                        // We can get here at design time, in the preview pipeline. In this case, generating exactly correct code
                        // is not important. At compile time, an invalid symbol would result in a failed compilation, which is ok.

                        if ( symbolInfo.CandidateSymbols.Length == 1 )
                        {
                            referencedSymbol = symbolInfo.CandidateSymbols[0];
                        }
                        else
                        {
                            return;
                        }
                    }

                    IMethodSymbol? localFunction = null;

                    if ( this._localFunctionStack.Count > 0 )
                    {
                        localFunction = this._localFunctionStack.Peek();
                    }

                    var resolvedReference = this._referenceResolver.Resolve(
                        this._containingSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                        localFunction,
                        referencedSymbol,
                        (ExpressionSyntax) node,
                        aspectReference,
                        this._semanticModel );

                    this.AspectReferences.Add( resolvedReference );
                }

                base.VisitCore( node );

                static MemberBindingExpressionSyntax GetConditionalMemberName( ConditionalAccessExpressionSyntax conditionalAccess )
                {
                    var walker = new ConditionalAccessExpressionWalker();

                    walker.Visit( conditionalAccess );

                    return walker.FoundName.AssertNotNull();
                }
            }

            private sealed class ConditionalAccessExpressionWalker : SafeSyntaxWalker
            {
                private ConditionalAccessExpressionSyntax? _context;

                public MemberBindingExpressionSyntax? FoundName { get; private set; }

                public override void VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
                {
                    if ( this._context == null )
                    {
                        this._context = node;

                        this.Visit( node.WhenNotNull );

                        this._context = null;
                    }
                }

                public override void VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
                {
                    if ( this._context != null )
                    {
                        this.FoundName = node;
                    }
                }
            }
        }
    }
}