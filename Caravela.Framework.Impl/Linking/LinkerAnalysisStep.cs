// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Analysis step of the linker, main goal of which is to produce LinkerAnalysisRegistry.
    /// </summary>
    internal partial class LinkerAnalysisStep : AspectLinkerPipelineStep<LinkerIntroductionStepOutput, LinkerAnalysisStepOutput>
    {
        public static LinkerAnalysisStep Instance { get; } = new();

        private LinkerAnalysisStep() { }

        public override LinkerAnalysisStepOutput Execute( LinkerIntroductionStepOutput input )
        {
            var referenceCounters = new Dictionary<SymbolVersion, int>();
            var methodBodyInfos = new Dictionary<ISymbol, MemberAnalysisResult>();

            foreach ( var syntaxTree in input.IntermediateCompilation.SyntaxTrees )
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
                            targetLayer = linkerAnnotation.AspectLayer;

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    // Increment the usage count.
                    var symbolInfo = input.IntermediateCompilation.Compilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode );

                    if ( symbolInfo.Symbol == null )
                    {
                        continue;
                    }

                    var symbolVersion = new SymbolVersion( symbolInfo.Symbol.AssertNotNull(), targetLayer );

                    referenceCounters.TryGetValue( symbolVersion, out var counter );
                    referenceCounters[symbolVersion] = counter + 1;
                }
            }

            // TODO: Do this on demand in analysis registry (provide the implementing class to the registry, let the registry manage the cache).
            // Analyze introduced method bodies.
            foreach ( var introducedMember in input.IntroductionRegistry.GetIntroducedMembers() )
            {
                var symbol = (IMethodSymbol) input.IntroductionRegistry.GetSymbolForIntroducedMember( introducedMember );

                // TODO: partial methods.
                var methodBodyVisitor = new MethodBodyWalker();
                methodBodyVisitor.Visit( introducedMember.Syntax );

                methodBodyInfos[symbol] = new MemberAnalysisResult(
                    symbol.ReturnsVoid ? methodBodyVisitor.ReturnStatementCount == 0 : methodBodyVisitor.ReturnStatementCount <= 1 );

                // var declarationSyntax = (MethodDeclarationSyntax) symbol.DeclaringSyntaxReferences.Single().GetSyntax();
                // ControlFlowGraph cfg = ControlFlowGraph.Create( declarationSyntax, this._intermediateCompilation.GetSemanticModel( declarationSyntax.SyntaxTree ) );
            }

            foreach ( var symbol in input.IntroductionRegistry.GetOverriddenMembers() )
            {
                var methodBodyVisitor = new MethodBodyWalker();
                methodBodyVisitor.Visit( symbol.DeclaringSyntaxReferences.Single().GetSyntax() );

                methodBodyInfos[symbol] = new MemberAnalysisResult(
                    symbol.ReturnsVoid ? methodBodyVisitor.ReturnStatementCount == 0 : methodBodyVisitor.ReturnStatementCount <= 1 );
            }

            var analysisRegistry = new LinkerAnalysisRegistry( input.IntroductionRegistry, input.OrderedAspectLayers, referenceCounters, methodBodyInfos );

            return new LinkerAnalysisStepOutput( input.Diagnostics, input.IntermediateCompilation, analysisRegistry );
        }
    }
}