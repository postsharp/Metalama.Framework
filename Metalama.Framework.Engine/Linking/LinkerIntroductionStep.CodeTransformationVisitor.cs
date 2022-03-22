// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerIntroductionStep
    {
        private class CodeTransformationVisitor : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private Context _currentContext;

            internal List<CodeTransformationMark> Marks { get; }

            public CodeTransformationVisitor( SemanticModel semanticModel, IReadOnlyList<ICodeTransformation> transformations)
            {
                this._semanticModel = semanticModel;
                this.Marks = new List<CodeTransformationMark>();
                this._currentContext = new Context( transformations );
            }

            public override void Visit( SyntaxNode? node )
            {
                base.Visit( node );
            }

            public override void VisitBlock( BlockSyntax node )
            {
                this.VisitBody( node, n => base.VisitBlock(n) );
            }

            public override void VisitArrowExpressionClause( ArrowExpressionClauseSyntax node )
            {
                this.VisitBody( node, n => base.VisitArrowExpressionClause( n ) );
            }

            private void VisitBody<T>( T? node, Action<T> visitBase )
                where T : SyntaxNode
            {
                if ( node == null || node.Parent == null || !(this._semanticModel.GetDeclaredSymbol( node.Parent ) is not null and var symbol) )
                {
                    return;
                }

                var previousContext = this._currentContext;

                try
                {
                    var unrejectedTransformations = previousContext.CodeTransformations.ToBuilder();
                    foreach ( var transformation in previousContext.CodeTransformations )
                    {
                        if (!SymbolEqualityComparer.Default.Equals(transformation.TargetDeclaration.GetSymbol(), symbol))
                        {
                            continue;
                        }

                        var context = new CodeTransformationContext( transformation, node );
                        transformation.EvaluateSyntaxNode( context );

                        if ( context.IsDeclined )
                        {
                            unrejectedTransformations.Remove( transformation );
                        }

                        this.Marks.AddRange( context.Marks );
                    }

                    this._currentContext = new Context( unrejectedTransformations.ToImmutable() );

                    if ( unrejectedTransformations.Count > 0 )
                    {
                        visitBase( node );
                    }
                }
                finally
                {
                    this._currentContext = previousContext;
                }
            }

            internal class Context
            {
                public ImmutableList<ICodeTransformation> CodeTransformations { get; }

                public Context( IReadOnlyList<ICodeTransformation> transformations)
                {
                    var builder = ImmutableList.CreateBuilder<ICodeTransformation>();
                    builder.AddRange( transformations );
                    this.CodeTransformations = builder.ToImmutable();
                }
            }
        }
    }
}