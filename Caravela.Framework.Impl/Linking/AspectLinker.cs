using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class AspectLinker
    {
        private readonly AdviceLinkerInput _input;

        public AspectLinker( AdviceLinkerInput input )
        {
            this._input = input;
        }

        public AdviceLinkerResult ToResult()
        {
            var intermediateCompilation = this._input.Compilation;

            var transformationsBySyntaxTree =
                this._input.CompilationModel.ObservableTransformations.Values.OfType<ISyntaxTreeIntroduction>()
                    .Concat( this._input.NonObservableTransformations.OfType<ISyntaxTreeIntroduction>() )
                    .GroupBy( t => t.TargetSyntaxTree, t => t )
                    .ToDictionary( g => g.Key, g => g );


            // TODO: This is not optimal structure for this.
            ImmutableMultiValueDictionary<ICodeElement, IntroducedMember> overrideLookup =
                ImmutableMultiValueDictionary<ICodeElement, IntroducedMember>.Empty.WithKeyComparer( CodeElementEqualityComparer.Instance );

            // First pass. Add all transformations to the compilation, but we don't link them yet.


            var newSyntaxTrees = new List<SyntaxTree>( transformationsBySyntaxTree.Count );
            foreach ( var syntaxTreeGroup in transformationsBySyntaxTree )
            {
                var oldSyntaxTree = syntaxTreeGroup.Key;

                AddIntroducedElementsRewriter addIntroducedElementsRewriter = new( syntaxTreeGroup.Value );

                var newRoot = addIntroducedElementsRewriter.Visit( oldSyntaxTree.GetRoot() );

                var newSyntaxTree = oldSyntaxTree.WithRootAndOptions( newRoot, oldSyntaxTree.Options );
                newSyntaxTrees.Add( newSyntaxTree );

                intermediateCompilation = intermediateCompilation.ReplaceSyntaxTree( oldSyntaxTree, newSyntaxTree );

                // TODO: This is slow.
                overrideLookup = overrideLookup.Merge( addIntroducedElementsRewriter.ElementOverrides );
            }

            // Second pass. Count references to modified methods.
            Dictionary<(ISymbol Symbol, int Version), int> referenceCounts = new();
            List<(AspectPart AspectPart, int Version)> aspectParts = new();
            aspectParts.AddRange( this._input.OrderedAspectParts.Select( ( ar, i ) => (ar, i + 1) ) );

            foreach ( var syntaxTree in newSyntaxTrees )
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
                            var originatingVersion = aspectParts.Where(
                                    p => p.AspectPart.AspectType.Name == linkerAnnotation.AspectTypeName && p.AspectPart.PartName == linkerAnnotation.PartName )
                                .Select( p => p.Version ).First();
                            targetVersion = originatingVersion + 1;
                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    // Increment the usage count.
                    var symbolInfo = intermediateCompilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode );

                    if ( symbolInfo.Symbol == null )
                        continue;

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

            var resultingCompilation = intermediateCompilation;

            // Third pass. Linking.
            // Two things it should do:
            //   1. Replace calls to the vanilla method to the call to the right "override" method.

            OverrideOrderRewriter rewriter = new OverrideOrderRewriter( intermediateCompilation, this._input.OrderedAspectParts);

            foreach ( var syntaxTree in intermediateCompilation.SyntaxTrees )
            {
                var newRoot = rewriter.Visit( syntaxTree.GetRoot() );

                var newSyntaxTree = syntaxTree.WithRootAndOptions( newRoot, syntaxTree.Options );

                resultingCompilation = resultingCompilation.ReplaceSyntaxTree( syntaxTree, newSyntaxTree );
            }

            return new AdviceLinkerResult( intermediateCompilation, Array.Empty<Diagnostic>() );
        }
    }
}
