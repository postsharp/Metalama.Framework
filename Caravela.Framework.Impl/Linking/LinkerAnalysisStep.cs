// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private readonly IReadOnlyList<OrderedAspectLayer> _orderedAspectLayers;
        private readonly Compilation _intermediateCompilation;
        private readonly LinkerTransformationRegistry _transformationRegistry;

        public LinkerAnalysisStep( CSharpCompilation intermediateCompilation, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers, LinkerTransformationRegistry transformationRegistry )
        {
            this._intermediateCompilation = intermediateCompilation;
            this._orderedAspectLayers = orderedAspectLayers;
            this._transformationRegistry = transformationRegistry;
        }

        public static LinkerAnalysisStep Create( CSharpCompilation intermediateCompilation, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers, LinkerTransformationRegistry transformationRegistry )
        {
            return new LinkerAnalysisStep( intermediateCompilation, orderedAspectLayers, transformationRegistry );
        }

        public LinkerAnalysisStepResult Execute()
        {
            var analysisRegistry = new LinkerAnalysisRegistry( this._transformationRegistry, this._orderedAspectLayers );

            foreach ( var syntaxTree in this._intermediateCompilation.SyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot().GetAnnotatedNodes( LinkerAnnotationExtensions.AnnotationKind ) )
                {
                    var linkerAnnotation = referencingNode.GetLinkerAnnotation().AssertNotNull();
                    AspectLayerId? targetLayer;

                    // Determine which version of the semantic is being invoked.
                    switch ( linkerAnnotation.Order )
                    {
                        case LinkerAnnotationOrder.Original: // Original
                            targetLayer = null;
                            break;

                        case LinkerAnnotationOrder.Default: // Next one.
                            targetLayer = linkerAnnotation.AspectLayerId;
                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    // Increment the usage count.
                    var symbolInfo = this._intermediateCompilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode );

                    if ( symbolInfo.Symbol == null )
                    {
                        continue;
                    }

                    var symbol = symbolInfo.Symbol.AssertNotNull();

                    analysisRegistry.AddReferenceCount( symbol, targetLayer );
                }
            }

            // TODO: Do thbis on demand in analysis registry.
            // Analyze introduced method bodies.
            foreach ( var introducedMember in this._transformationRegistry.GetIntroducedMembers() )
            {
                var symbol = (IMethodSymbol) this._transformationRegistry.GetSymbolForIntroducedMember( introducedMember );

                // TODO: partial methods.
                var methodBodyVisitor = new MethodBodyWalker();
                methodBodyVisitor.Visit( introducedMember.Syntax );
                analysisRegistry.SetBodyAnalysisResults( symbol, symbol.ReturnsVoid ? methodBodyVisitor.ReturnStatementCount == 0 : methodBodyVisitor.ReturnStatementCount <= 1 );

                // var declarationSyntax = (MethodDeclarationSyntax) symbol.DeclaringSyntaxReferences.Single().GetSyntax();
                // ControlFlowGraph cfg = ControlFlowGraph.Create( declarationSyntax, this._intermediateCompilation.GetSemanticModel( declarationSyntax.SyntaxTree ) );
            }

            foreach (var symbol in this._transformationRegistry.GetOverriddenMethods())
            {
                var methodBodyVisitor = new MethodBodyWalker();
                methodBodyVisitor.Visit( symbol.DeclaringSyntaxReferences.Single().GetSyntax() );
                analysisRegistry.SetBodyAnalysisResults( symbol, symbol.ReturnsVoid ? methodBodyVisitor.ReturnStatementCount == 0 : methodBodyVisitor.ReturnStatementCount <= 1 );
            }

            return new LinkerAnalysisStepResult( analysisRegistry );
        }
    }
}
