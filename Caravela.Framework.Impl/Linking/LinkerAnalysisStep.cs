// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.AspectOrdering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerAnalysisStep
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
            var referenceRegistry = new LinkerAnalysisRegistry( this._transformationRegistry, this._orderedAspectLayers );

            Dictionary<(ISymbol Symbol, int Version), int> referenceCounts = new();
            List<(AspectLayer AspectLayer, int Version)> aspectLayers = new();
            aspectLayers.AddRange( this._orderedAspectLayers.Select( ( ar, i ) => ((AspectLayer)ar, i + 1) ) );

            foreach ( var syntaxTree in this._intermediateCompilation.SyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot().GetAnnotatedNodes( LinkerAnnotationExtensions.AnnotationKind ) )
                {
                    var linkerAnnotation = referencingNode.GetLinkerAnnotation()!;
                    int targetVersion;

                    // Determine which version of the semantic is being invoked.
                    switch ( linkerAnnotation.Order )
                    {
                        case LinkerAnnotationOrder.Original:
                            targetVersion = 0;
                            break;

                        case LinkerAnnotationOrder.Default: // Next one.
                            var originatingVersion = aspectLayers.Where(
                                    p => p.AspectLayer.AspectLayerId == linkerAnnotation.AspectLayerId )
                                .Select( p => p.Version ).First();
                            targetVersion = originatingVersion + 1;
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
                    var symbolVersion = (symbol, targetVersion);

                    if ( referenceCounts.TryGetValue( symbolVersion, out var count ) )
                    {
                        referenceCounts[symbolVersion] = count + 1;
                    }
                    else
                    {
                        referenceCounts[symbolVersion] = 1;
                    }
                }
            }

            return new LinkerAnalysisStepResult( referenceRegistry );
        }
    }
}
