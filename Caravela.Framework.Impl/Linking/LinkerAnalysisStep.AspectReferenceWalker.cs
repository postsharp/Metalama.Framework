// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerAnalysisStep
    {
        // TODO: Change this to counting return statements that change the control flow.

        /// <summary>
        /// Walks method bodies, counting return statements.
        /// </summary>
        private class AspectReferenceWalker : CSharpSyntaxWalker
        {
            private readonly AspectReferenceResolver _referenceResolver;
            private readonly SemanticModel _semanticModel;
            private readonly ISymbol _containingSymbol;

            public List<ResolvedAspectReference> AspectReferences { get; }

            public AspectReferenceWalker( AspectReferenceResolver referenceResolver, SemanticModel semanticModel, ISymbol containingSymbol )
            {
                this._referenceResolver = referenceResolver;
                this._semanticModel = semanticModel;
                this.AspectReferences = new List<ResolvedAspectReference>();
                this._containingSymbol = containingSymbol;
            }

            public override void Visit( SyntaxNode? node )
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

                    var resolvedReference = this._referenceResolver.Resolve(
                        this._containingSymbol,
                        referencedSymbol,
                        (ExpressionSyntax) node,
                        aspectReference );

                    this.AspectReferences.Add( resolvedReference );
                }

                base.Visit( node );

                static MemberBindingExpressionSyntax GetConditionalMemberName( ConditionalAccessExpressionSyntax conditionalAccess )
                {
                    var walker = new ConditionalAccessExpressionWalker();

                    walker.Visit( conditionalAccess );

                    return walker.FoundName.AssertNotNull();
                }
            }

            private class ConditionalAccessExpressionWalker : CSharpSyntaxWalker
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